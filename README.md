# ChatR

## ChatR.Server (Backend)
This is a regular dotnet8 web-api application.
### Compile
In the root folder:
```bash
dotnet build
```
### Run 
Again, in the root folder:
```bash
 dotnet run -project .\ChatR.Server\
```
App is now running on ` http://localhost:5067`.

## ChatR.Server (Backend)
This is a simple [Elm](https://elm-lang.org/) application, which compiles (well, transpiles really) to a static `.js` file.
The **SignalR** client logic is to be found in `chat.js`.
The full distribution, including html and css is at `./ChatR.Client/public`.
### Compile the Elm Code
  * For distribution:
  ```bash
  elm make src/Main.elm  --output=public/main.js --optimize
  ```
  * During local dev:
  ```bash
  elm-live src/Main.elm --pushstate --dir=public --start-page=index.html --open '--' --output=public/main.js --optimize
  ```
  Here you can replace `--optimize` with `--debug` to get acces to the time travel debugger if needed.
### Run
  * Navigate to `ChatR.Client/`
  * `npm install`
  * `npm start`

  
Make sure you load the right `config.*.json` depending on where you want the client to connect to, at the top of chat.js.