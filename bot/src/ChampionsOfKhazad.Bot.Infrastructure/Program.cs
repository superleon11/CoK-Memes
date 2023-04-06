using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.Docker;
using Pulumi.Docker.Inputs;
using Pulumi.Random;
using Config = Pulumi.Config;
using ContainerArgs = Pulumi.AzureNative.App.Inputs.ContainerArgs;
using Resource = Pulumi.Resource;
using SecretArgs = Pulumi.AzureNative.App.Inputs.SecretArgs;

return await Pulumi.Deployment.RunAsync(() =>
{
    var config = new Config();
    var providerConfig = new Config("azure-native");

    var resourceGroup = new ResourceGroup(
        "resource-group",
        new ResourceGroupArgs { ResourceGroupName = "cok-memes" },
        new CustomResourceOptions
        {
            ImportId =
                $"/subscriptions/{providerConfig.Require("subscriptionId")}/resourceGroups/cok-memes",
            Protect = true
        }
    );

    var logAnalytics = new Workspace(
        "log-analytics",
        new WorkspaceArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new WorkspaceSkuArgs { Name = WorkspaceSkuNameEnum.PerGB2018 },
            RetentionInDays = 30
        }
    );

    var logAnalyticsSharedKeys = Output
        .Tuple(resourceGroup.Name, logAnalytics.Name)
        .Apply(
            items =>
                GetSharedKeys.InvokeAsync(
                    new GetSharedKeysArgs
                    {
                        ResourceGroupName = items.Item1,
                        WorkspaceName = items.Item2,
                    }
                )
        );

    var environment = new ManagedEnvironment(
        "environment",
        new ManagedEnvironmentArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = "log-analytics",
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = logAnalytics.CustomerId,
                    SharedKey = logAnalyticsSharedKeys.Apply(r => r.PrimarySharedKey!)
                }
            }
        }
    );

    const string imageRegistryServer = "docker.io";
    var imageRegistryUsername = config.Require("imageRegistryUsername");

    Image CreateImage(string name, Input<string> tag, Resource? dependsOn = null) =>
        new(
            name,
            new ImageArgs
            {
                ImageName = Output.Format($"uncledave/cok-bot:{tag}"),
                Build = new DockerBuildArgs
                {
                    Context = "../ChampionsOfKhazad.Bot/",
                    Platform = "linux/amd64"
                },
                Registry = new RegistryArgs
                {
                    Server = imageRegistryServer,
                    Username = imageRegistryUsername,
                    Password = config.RequireSecret("imageRegistryWritePassword")
                }
            },
            new CustomResourceOptions { DependsOn = dependsOn }
        );

    var uniqueImageTag = new RandomUuid("unique-bot-image-id");
    var botImage = CreateImage("bot-image", "latest");
    var uniqueBotImage = CreateImage("unique-bot-image", uniqueImageTag.Result, botImage);

    const string botTokenSecretName = "bot-token";
    const string imageRegistryReadPasswordSecretName = "registry-read-password";

    var containerApp = new ContainerApp(
        "bot-app",
        new ContainerAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ManagedEnvironmentId = environment.Id,
            Configuration = new ConfigurationArgs
            {
                Registries = new RegistryCredentialsArgs
                {
                    Server = imageRegistryServer,
                    Username = imageRegistryUsername,
                    PasswordSecretRef = imageRegistryReadPasswordSecretName
                },
                Secrets =
                {
                    new SecretArgs
                    {
                        Name = botTokenSecretName,
                        Value = config.RequireSecret("botToken")
                    },
                    new SecretArgs
                    {
                        Name = imageRegistryReadPasswordSecretName,
                        Value = config.RequireSecret("imageRegistryReadPassword")
                    }
                }
            },
            Template = new TemplateArgs
            {
                Containers = new ContainerArgs
                {
                    Name = "bot",
                    Image = uniqueBotImage.ImageName,
                    Env =
                    {
                        new EnvironmentVarArgs
                        {
                            Name = "Bot__Token",
                            SecretRef = botTokenSecretName
                        },
                        new EnvironmentVarArgs
                        {
                            Name = "DOTNET_ENVIRONMENT",
                            Value = config.Require("environment")
                        }
                    }
                },
                Scale = new ScaleArgs { MinReplicas = 1, MaxReplicas = 1 }
            }
        }
    );
});
