FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Development
ENV TZ="Asia/Shanghai"
COPY . .
ENTRYPOINT ["dotnet", "SolutionName.HttpApi.Host.dll"]