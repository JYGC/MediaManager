namespace MediaManager.Logging

open System
open System.IO

module TextLogging =
    let createGetFileStream logFilePath =
        let fileStream =
            if File.Exists logFilePath then
                File.AppendText logFilePath
            else
                File.CreateText logFilePath
        let getFileStream() = fileStream
        getFileStream

    let writeLog (getFileStream: unit -> StreamWriter) (logText: string) (logDateTime: DateTime) =
        let fileStream = getFileStream()
        fileStream.WriteLine $"\r\nDATETIME: {logDateTime.ToLongTimeString()} {logDateTime.ToLongDateString()}"
        fileStream.WriteLine "ERROR:"
        fileStream.WriteLine $"{logText}"
        fileStream.WriteLine "-------------------------------"
        fileStream.Flush()