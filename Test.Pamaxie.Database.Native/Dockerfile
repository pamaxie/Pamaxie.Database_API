FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Test.Pamaxie.Database.Native/Test.Pamaxie.Database.Native.csproj", "Test.Pamaxie.Database.Native/"]
COPY ["Pamaxie.Database.Design/Pamaxie.Database.Design.csproj", "Pamaxie.Database.Design/"]
COPY ["Pamaxie.Database.Native/Pamaxie.Database.Native.csproj", "Pamaxie.Database.Native/"]
RUN dotnet restore "Test.Pamaxie.Database.Native/Test.Pamaxie.Database.Native.csproj"
COPY . .
WORKDIR "/src/Test.Pamaxie.Database.Native"
RUN dotnet build "Test.Pamaxie.Database.Native.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Test.Pamaxie.Database.Native.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV PamRedisDbVar="Testing"
ENV PamPgSqlDbVar="Testing2"
ENTRYPOINT ["dotnet", "Test.Pamaxie.Database.Native.dll"]
