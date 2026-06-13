FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .

RUN dotnet publish $(find . -name "*.csproj") -c Release -o out

RUN find . -name "cookies.txt" -exec cp {} /app/out/cookies.txt \;

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

RUN apt-get update && \
    apt-get install -y ffmpeg python3 python3-pip nodejs npm && \
    python3 -m pip install yt-dlp --break-system-packages && \
    ln -s /usr/bin/nodejs /usr/bin/node && \
    apt-get clean && rm -rf /var/lib/apt/lists/*


COPY --from=build /app/out .

RUN mkdir -p /app/Downloads && chmod 777 /app/Downloads

ENTRYPOINT ["dotnet", "TelegramBotYtMusic.dll"]