# Estágio Build - Usando a imagem do .NET SDK para compilar e publicar a aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia todos os arquivos da raiz do projeto para dentro do contêiner
COPY . .

# Restaura as dependências da solução (garante que o arquivo .sln e o .csproj sejam encontrados)
RUN dotnet restore ProcessService.APP/ProcessService.APP.csproj

# Publica a aplicação especificada em Release
RUN dotnet publish ProcessService.APP/ProcessService.APP.csproj -c Release -o out

# Estágio Final - Usando a imagem do .NET ASP.NET para a execução do serviço
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copia os arquivos publicados do estágio build para o estágio final
COPY --from=build /app/out .

# Define o ponto de entrada do contêiner para rodar o serviço
ENTRYPOINT ["dotnet", "ProcessService.APP.dll"]
