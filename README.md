# Pamaxie.Database
### It is with a hard feeling in our heart, that we have to announce, that Pamaxie as a project is cancelled. Due to lacking funds and massive changes in the project owners life (Leejja) we currently no longer have the financial or time capacity to maintain it which is why we think it is best that we close this chapter of our life. We are sorry that we could not live up to our promises. We are looking into a cheap way to host our dataset. If possible we may update this page. Due to concerns for the public we might not however (there is some graphic imagry in there which may be quite disturbing or problematic to release)

[![Build .Net](https://github.com/pamaxie/Pamaxie.Database/actions/workflows/dotnet.yml/badge.svg)](https://github.com/pamaxie/Pamaxie.Database/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/pamaxie/Pamaxie.Database/actions/workflows/codeQL.yml/badge.svg)](https://github.com/pamaxie/Pamaxie.Database/actions/workflows/codeQL.yml)
[![Publish Docker image](https://github.com/pamaxie/Pamaxie.Database/actions/workflows/docker-image.yml/badge.svg)](https://github.com/pamaxie/Pamaxie.Database/actions/workflows/docker-image.yml)

## About

This project contains all things related to our Database API (except the Data structure which can be found [here](https://github.com/pamaxie/Pamaxie.Data)).
We also have something we call "dynamic database drivers" which means during the configuration of the database API our service looks inside the folder for anything implementing our Database Structure if they do, they can implement their own Database Driver for our API allowing you to use whatever database you want (if you implement the driver for it). We currently use redis so this is what we are providing here. We might implement different database drivers in the future (no promises).

If you want to make your own Database Driver, you can have a look at the Database Drivers folder and look at how we implemented our Redis driver. That should help you get started. We will add detailed explanation on how to make a custom driver in the future.

#### Possible thanks to:

![**Federal Minstry Of Research and education**](https://i.imgur.com/riyuVGf.jpg) ![**Federal Minstry Of Research and education**](https://i.imgur.com/GI9XILN.png)

#### Thanks to these partners helping us keep this project alive:

![**eclips.is**](https://eclips.is/images/logo.png)
