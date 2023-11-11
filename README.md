# elib-project
![](https://img.shields.io/badge/WORK_IN_PROGRESS-red.svg)
![](https://img.shields.io/badge/Price-Free-brightgreen.svg)
![](https://img.shields.io/badge/License-GPL3.0-blue.svg)

**ELib** is an application for organizing ebook files on a computer. It's created using .NET 6 and WPF. The idea is to have a basic ebook manager, sort of like a dumbed down version of Calibre.

Features:
* Organizing books by authors, series
* Adding books to custom collections
* Filtering and sorting of specific book views
* Parsing ebook files for metadata to speed up importing
* Using **Google Books API** to fetch book info based on ISBN number
* Export books in organized hierarchy and file names
* Mark books as **Read** or **Favorite**
* Specify external reader

The project uses **SQLite database** for saving book info and relationships. Access to database is done with a generic repository pattern using **Dapper**. The application features a custom Window header, and the user interface is done with custom WPF user controls using **Model-View-ViewModel pattern**, designed to look modern and functional. The app also supports keyboard navigation for the most part.

The app development is still in progress but here are some current screenshots:


<p align="center"><img src="./Screenshots/MainScreenshot.png?raw=true" title="file sharing" align="center" hspace="5" vspace="5">
<p align="center"><img src="./Screenshots/BookView.png?raw=true" title="file sharing" align="center" hspace="5" vspace="5">
