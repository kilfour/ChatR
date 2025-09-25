1. Hello Hub
    - Create Server Project:
    ```bash
    dotnet new webapi -n ChatR.Server --use-minimal-apis
    ```
    - Add SignalR
    ```bash
    dotnet add package Microsoft.AspNetCore.SignalR
    ```
    - Configure SignalR in Program.cs
    - write a minimal client

3. Auth
    - Add dependency:
    ```bash
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
    ```