﻿module Rezoom.SQL.Test.TestVendorStatements
open System
open NUnit.Framework
open FsUnit
open Rezoom.SQL
open Rezoom.SQL.Mapping

let vendor (sql : string) =
    let userModel = userModel1()
    let parsed = CommandEffect.OfSQL(userModel.Model, "anonymous", sql)
    let indexer = dispenserParameterIndexer()
    let fragments = userModel.Backend.ToCommandFragments(indexer, parsed.Statements) |> List.ofSeq
    printfn "%A" fragments

[<Test>]
let ``vendor without exprs or imaginary`` () =
    vendor """
        vendor rzsql {
            this is raw text
        }
    """

[<Test>]
let ``vendor without imaginary`` () =
    vendor """
        vendor rzsql {
            raw text {@param1} more raw {@param2}
        }
    """

[<Test>]
let ``vendor with imaginary`` () =
    vendor """
        vendor rzsql {
            raw text {@param1} more raw {@param2}
        } imagine {
            select Id from Users
        }
    """

[<Test>]
let ``vendor with wacky delimiters`` () =
    vendor """
        vendor rzsql [:<#
            raw text [:<# @param1 #>:] more raw [:<# @param2 #>:]
        #>:] imagine [:<#
            select Id from Users
        #>:]
    """