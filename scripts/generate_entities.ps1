"Navigating to project."
cd ../src

"Getting user secrets from .NET solution."
dotnet user-secrets list
$HOME_ASSISTANT_HOST=$(dotnet user-secrets get "HOME_ASSISTANT_HOST")
$HOME_ASSISTANT_TOKEN=$(dotnet user-secrets get "HOME_ASSISTANT_TOKEN")

"Generating Home Assistant entities."
nd-codegen -host $HOME_ASSISTANT_HOST -token $HOME_ASSISTANT_TOKEN

"Navigating back to working directory."
cd ../scripts