# SAFE Template
This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites
You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download) 3.1 or higher.
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
**Important: Files from *Shared.fs* are currently not under watch of webpack. Modify something in the server files then something in _Index.fs_**

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/docs/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
* [Fable.SignalR](https://shmew.github.io/Fable.SignalR/#/)
* [Feliz](https://zaid-ajaj.github.io/Feliz/)
* [Feliz.Bulma](https://dzoukr.github.io/Feliz.Bulma/)
* [Bulma](https://bulma.io/documentation/)