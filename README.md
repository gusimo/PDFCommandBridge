# PDFCommandBridge
Will read invisible Control-Texts from a PDF. Transforms them to a command and runs this command on shell.

## What is it good for?

You are using an old software, which is not capable of sending  email natively, but you want to provide your offers or invoices by email easily, including automatic setting of receivers email address? You have a report designer-tool, to create custom reports, lets say for invoices, etc?
With PDFCommandbridge you can create invisible control-commands in your Report-Templates (how: see below). Then you can send them to a pdf printer. Use a PDF-Printer with configurable "after-print-actions" or a folder-watching tool, to run PDFCommandbridge afterwards. It will extract invisible parameters from the report and translate them to a custom command that will be run on the shell.

## Example

### Invisible Characters in Invoice-Report
Print the Email-Field (or any custom field, used for email) to your Invoice-Report Template. Use white text color to make them invisible. Use this format: ``%%ParameterName: someValue%%``.

### PDF Printer
I used eDocPrintPro from this page: https://www.pdfprinter.at/. It will allow you to run a custom action after printing the PDF.

### Run PDF command bridge
Just pass the file name of a pdf
``pdfcommandridge "some document.pdf"``

### PDF Command bridge config
There is a settings.json file (will be created after running the first time, if not available)

      {
	      "testmode": true,
	      "command": "thunderbird",
		  "arguments": "-compose \"to='%%EmailTo%%', cc='', subject='%%subject%%', body='%%body%%',attachment='%%FullPath%%,%%EmailAttach%%'\""
    }

- testmode will only pause and show the command that will be run on shell. If testmode is set to false, it will not pause and it will run the command.
- command enter the shell command you want to run, filled with parameters from your pdf.
- arguments are analog to command the commands parameters
- you can also use Environment parameters in the same format. For Example %%username%% to put the windows-username into the command.

This example command will run thunderbird (included to PATH-Variable, but not needed if you are not lazy) and it will compose a new email.