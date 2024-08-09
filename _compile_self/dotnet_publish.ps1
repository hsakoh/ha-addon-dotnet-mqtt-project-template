dotnet restore "src/EXAMPLEMqttApp/EXAMPLEMqttApp.csproj"
dotnet publish "src/EXAMPLEMqttApp/EXAMPLEMqttApp.csproj" -r linux-musl-arm64 -p:PublishSingleFile=true --self-contained false -c Release -o "./_compile_self/aarch64" --no-restore
dotnet publish "src/EXAMPLEMqttApp/EXAMPLEMqttApp.csproj" -r linux-musl-x64 -p:PublishSingleFile=true --self-contained false -c Release -o "./_compile_self/amd64" --no-restore
dotnet publish "src/EXAMPLEMqttApp/EXAMPLEMqttApp.csproj" -r linux-musl-arm -p:PublishSingleFile=true --self-contained false -c Release -o "./_compile_self/armv7" --no-restore
