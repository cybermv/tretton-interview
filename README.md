# Scrappy
```a tretton37 job interview task```

## Introduction
This is a console application which can be used to scrape a website recursively. It takes two mandatory parameters - the website base URL and the output folder where downloaded files will be put.

The app will download the index page and, recursively, every local page that the index links to. From there it will continue it's recursion until it gets to a page where there are no more non-processed links on the website. Also, it will download all local page resources (CSS, JS, images and videos).

It is a .NET 5.0 application and it can be run using the ```dotnet``` tool or by executing the compiled .exe through the command line. Compiling is done with ```dotnet build``` or by opening the solution in Visual Studio and then building it. If the console app is invoked without parameters it will display a help screen.

## Examples
Examples how to run a web scrape of the tretton37 website:
```
dotnet run -- -u https://tretton37.com/ -o D:\ScrapingOutput

Vele.Scrappy.ConsoleApp.exe -u https://tretton37.com/ -o D:\ScrapingOutput
```

The console app will write it's progress to the standard output. In case of success, it will exit with ```0```; in case of error it will exit with ```-1```.

## Thought process while doing the assignment
```
Downloading an HTML sounds easy enough, plus it's dependencies... it's just CSS and JS, and maybe images, videos, music... ok it's not 'just'. And some of it might be on other domains too. But this has to be done N times for a website!? Sure, we can scrape 'a' tags to get new URL's to scrape... Also resources will be shared between pages, and we might encounter navigation loops... duplicates should be easy to avoid if we maintain a unique list of scraped items - wait, concurrency?? Damn, we have to make sure threads (.NET has also tasks hmm) don't step on each others' toes! But - at least saving files will be easy!... wait, where's the .html extension? There's a query string in the href attribute?  ...
This will never work.
- And finally, it works. With a few bugfeatures :)
```

## Technical details

### Project structure
* Vele.Scrappy - class library project containing classes with processing logic
* Vele.Scrappy.ConsoleApp - executable project containing the entry point and calling the library code

### Pseudo code
1. Validate all parameters
2. Initiate the scraping process
   1. Start the task of scraping the pages recursively - starting from website root
   2. Start the task of persisting downloaded pages and resources to the filesystem
   3. When the task of scraping is completed, signal the file persisting task to conclude it's work
3. Print out files which had errors in processing
4. Finish the scraping process and exit

### Process of scraping
1. Download and parse HTML using HtmlAgilityPack
2. Mark the page as scraped
3. Extract all page links ('a' tags pointing to a local page, whether with a relative or absolute URL)
4. Extract all page resources (all tags besides 'a' which have a 'href' or 'src' attribute)
5. Initiate tasks of recursive scraping on page links
6. Initiate tasks of download of page resources and mark them as scraped
7. Wait until the tasks complete

### Process of persisting files
1. Take a file which has been scraped but not persisted
2. Determine the filename using the absolute URL (retaining the relative path from the website base URL)
3. Create the neccessary (sub)directories
4. Write the file to the filesystem
5. Mark file as persisted

### Thoughts, issues, improvement ideas
* DownloadExternal flag - enable downloading of external resources used in the website (e.g. photos stored on a CDN).
* ParallelExecutionCount flag - enable setting the amount of threads which will be used to process the tasks.
* What to do with http/https mismatches?
* ContinueOnError flag - determine what to do on download error.
* Filename managing - some web servers can serve pages without the '.html' extension, sometimes even without a filename (e.g. index.html). These files must be persisted using a default filename, or have an extension appended. But then links won't work anymore. The links pointing to it should be rewritten?
* Throttling - some sites will detect the huge network increase from a single client and might consider it a DOS attack. With throttling we can reduce the chances of that.
* RecursionDepth flag - we might want to limit the depth of traversing the website structure.
* Vele.Scrappy.Test project - unit tests for the library classes

### Libraries used
* HtmlAgilityPack for HTML parsing and traversing.
* CommandLine for command line arguments parsing and validating.
* Serilog for logging output to console.