namespace MediaManager.Initializations

open System

open OPMF.Settings
open MediaManager.Logging

module TextLogComposition =
    let textLogFilePath = ConfigHelper.ReadonlySettings.GetTextLogFile()
    let createGetFileStream() = TextLogging.createGetFileStream textLogFilePath
    
    let writeLog logText = TextLogging.writeLog (createGetFileStream()) logText DateTime.Now