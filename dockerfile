# Pull down an image from Docker Hub that includes the .NET core SDK: 
# This is so we have all the tools necessary to compile the app.
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

# Copy the source from your machine onto the app container.
WORKDIR /app
COPY . ./

# Install dependencies. 
RUN dotnet restore UltimateWebApi.sln
RUN dotnet build ./UltimateWebApi -c Release
# Compile, then pack the compiled app and dependencies into a deployable unit (ultimateAPI folder).
RUN dotnet publish ./UltimateWebApi/UltimateWebApi.csproj -c Release -o ultimateAPI --no-restore --no-build

# Pull down an image from Docker Hub that includes only the ASP.NET core runtime:
# We don't need the SDK anymore, so this will produce a lighter-weight image
# that can still run the app.
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /main
# Expose port 80 to your local machine so you can access the app.
EXPOSE 80
# Copy the published app to this new runtime-only container.
COPY --from=build-env /app/ultimateAPI .
#Adding this env variable for swagger support.
ENV ASPNETCORE_ENVIRONMENT=Development
# To run the app, run `dotnet UltimateWebApi.dll`, which we just copied over.
ENTRYPOINT ["dotnet", "UltimateWebApi.dll"] 