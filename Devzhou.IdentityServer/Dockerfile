FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app
COPY . .
EXPOSE 80
ENV TZ=Asia/Shanghai
ENTRYPOINT dotnet Devzhou.IdentityServer.dll