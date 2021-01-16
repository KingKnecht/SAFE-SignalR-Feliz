# Template for SAFE, SignalR, Feliz
I created this template to have a nice starting point to test some new ideas.
At the moment it supports:
* REST API + updates via SignalR to other clients when something has changed (e.g. Todo added)
* SignalR RPC API Elmish-style (Counter +/-)
* SignalR RPC API Feliz-React-style (Counter +/-)

Missing:
* Server streaming
* Client streaming

## Install pre-requisites
You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download) 3.1 (for Fable)
* The [.NET Core SDK](https://www.microsoft.com/net/download) 5.0 (for Server)
* [npm](https://nodejs.org/en/download/) package manager.
* [Node LTS](https://nodejs.org/en/download/).

## Starting the application
Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```
Open two terminals.

On first terminal do:
```bash
cd src/Server/
dotnet watch run
```
On second terminal do:
```bash
cd src/Client/
npm start
```
Click on the link provided by webpack in the terminal window to open a browser window. Open another browser window with the same URL to see updates.

**Important:** Files from *Shared.fsprj* are currently not under watch of webpack. Modify something in the server files then something in _Index.fs_

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/docs/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
* [Fable.SignalR](https://shmew.github.io/Fable.SignalR/#/)
* [Feliz](https://zaid-ajaj.github.io/Feliz/)
* [Feliz.Bulma](https://dzoukr.github.io/Feliz.Bulma/)
* [Bulma](https://bulma.io/documentation/)