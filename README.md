# DuplicateFileSearcher

## Intro

When I wrote this program, I did not trust Puran utilities 
(partly because of the lack of information regarding the used algorithm, 
partly due to the fact that I could not see the default options) and did not know
any good freeware programs for Windows.

Unfortunately I'm a bad designer.

## Advantages:
-  Portable (no installation required)
-  you Can specify filters by file size, file extension
-  you can choice hash function

## Algorithm:
-  Make lists with files which has one size
-  Calculate hash sums for each files from previous step
-  Output groups with files with one hash sum.

## Skills
-  DataGrid template modification (add ToolTip for fields, custom template, two coloring)
-  IValueConverter
-  Styles, StaticResources
-  ISerializable (Only for serialize)
-  Round Buttons
-  DataBind to ListBox (bind  ObservableCollection<string>)
-  Calculate hash for files

## System Requirements:
-  Windows 7 or high
- .NET Framework 4.5
