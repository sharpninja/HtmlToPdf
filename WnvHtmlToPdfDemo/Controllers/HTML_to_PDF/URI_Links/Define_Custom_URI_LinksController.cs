﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

// Use Winnovative Namespace
using Winnovative;

namespace WnvHtmlToPdfDemo.Controllers.HTML_to_PDF.URI_Links
{
    public class Define_Custom_URI_LinksController : Controller
    {
        [HttpPost]
        public ActionResult ConvertHtmlToPdf(IFormCollection collection)
        {
            // Create a HTML to PDF converter object with default settings
            HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();

            // Set license key received after purchase to use the converter in licensed mode
            // Leave it not set to use the converter in demo mode
            htmlToPdfConverter.LicenseKey = "fvDh8eDx4fHg4P/h8eLg/+Dj/+jo6Og=";

            // Set an adddional delay in seconds to wait for JavaScript or AJAX calls after page load completed
            // Set this property to 0 if you don't need to wait for such asynchcronous operations to finish
            htmlToPdfConverter.ConversionDelay = 2;

            byte[] outPdfBuffer = null;

            if (collection["HtmlPageSource"] == "convertHtmlRadioButton")
            {
                string htmlWithInternalLinksAttributes = collection["htmlStringTextBox"];
                string baseUrl = collection["baseUrlTextBox"];

                // Convert a HTML string with URI links to a PDF document in a memory buffer
                outPdfBuffer = htmlToPdfConverter.ConvertHtml(htmlWithInternalLinksAttributes, baseUrl);
            }
            else
            {
                string url = collection["urlTextBox"];

                // Convert a HTML page with URI links to a PDF document in a memory buffer
                outPdfBuffer = htmlToPdfConverter.ConvertUrl(url);
            }
            
            // Send the PDF file to browser
            FileResult fileResult = new FileContentResult(outPdfBuffer, "application/pdf");
            fileResult.FileDownloadName = "Custom_URI_Links.pdf";

            return fileResult;
        }
    }
}