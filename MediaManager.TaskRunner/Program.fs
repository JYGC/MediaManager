﻿open System
open System.Collections.Generic
open System.Text.Json
open MediaManager.Dtos.SiteProviderDtos
open MediaManager.Repositories

let newSiteProviders = new List<FullSiteProviderDto>()
newSiteProviders.Add(new FullSiteProviderDto(Guid.Empty, "Vemeo", "Vemeo.com"))
newSiteProviders.Add(new FullSiteProviderDto(Guid.Empty, "Youtube", "youtube.com"))
newSiteProviders.Add(new FullSiteProviderDto(Guid.Empty, "Dailymotion", "dailymotion.com"))

SiteProviderRepository.AddMultipleSiteProvider newSiteProviders
|> ignore

let names = new List<string>()
names.Add("Vimeo")
names.Add("Youtube")
let testvalue = SiteProviderRepository.GetSiteProviderByNames names

let jsonResult =
    testvalue
    |> JsonSerializer.Serialize

printfn "%s" jsonResult