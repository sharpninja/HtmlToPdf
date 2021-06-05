﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Hosting;
using System.Drawing;

// Use Winnovative Namespace
using Winnovative;

namespace WnvHtmlToPdfDemo.Controllers.PDF_Creator.Headers_and_Footers
{
    public class PDF_Creator_HTML_in_Header_FooterController : Controller
    {
        IFormCollection formCollection;

        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment m_hostingEnvironment;
        public PDF_Creator_HTML_in_Header_FooterController(IWebHostEnvironment hostingEnvironment)
        {
            m_hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public ActionResult CreatePdf(IFormCollection collection)
        {
            formCollection = collection;

            // Create a PDF document
            Document pdfDocument = new Document();

            // Set license key received after purchase to use the converter in licensed mode
            // Leave it not set to use the converter in demo mode
            pdfDocument.LicenseKey = "fvDh8eDx4fHg4P/h8eLg/+Dj/+jo6Og=";

            // Add a PDF page to PDF document
            PdfPage pdfPage = pdfDocument.AddPage();

            HtmlToPdfElement htmlToPdfElement = null;
            try
            {
                // Add document header
                if (collection["addHeaderCheckBox"].Count > 0)
                    AddHeader(pdfDocument, collection["drawHeaderLineCheckBox"].Count > 0);

                // Add document footer
                if (collection["addFooterCheckBox"].Count > 0)
                    AddFooter(pdfDocument, collection["addPageNumbersInFooterCheckBox"].Count > 0, collection["drawFooterLineCheckBox"].Count > 0);

                // Create a HTML to PDF element to add to document
                htmlToPdfElement = new HtmlToPdfElement(0, 0, collection["urlTextBox"]);

                // Optionally set a delay before conversion to allow asynchonous scripts to finish
                htmlToPdfElement.ConversionDelay = 2;

                // Install a handler where to change the header and footer in pages generated by the HTML to PDF element
                htmlToPdfElement.PrepareRenderPdfPageEvent += new PrepareRenderPdfPageDelegate(htmlToPdfElement_PrepareRenderPdfPageEvent);

                // Optionally add a space between header and the page body
                // Leave this option not set for no spacing
                htmlToPdfElement.Y = float.Parse(collection["firstPageSpacingTextBox"]);
                htmlToPdfElement.TopSpacing = float.Parse(collection["headerSpacingTextBox"]);

                // Optionally add a space between footer and the page body
                // Leave this option not set for no spacing
                htmlToPdfElement.BottomSpacing = float.Parse(collection["footerSpacingTextBox"]);

                // Add the HTML to PDF element to document
                // This will raise the PrepareRenderPdfPageEvent event where the header and footer visibilit per page can be changed
                pdfPage.AddElement(htmlToPdfElement);

                // Save the PDF document in a memory buffer
                byte[] outPdfBuffer = pdfDocument.Save();
                
                // Send the PDF file to browser
                FileResult fileResult = new FileContentResult(outPdfBuffer, "application/pdf");
                fileResult.FileDownloadName = "HTML_in_Header_Footer.pdf";

                return fileResult;
            }
            finally
            {
                // uninstall handler
                htmlToPdfElement.PrepareRenderPdfPageEvent -= new PrepareRenderPdfPageDelegate(htmlToPdfElement_PrepareRenderPdfPageEvent);

                // Close the PDF document
                pdfDocument.Close();
            }            
        }

        /// <summary>
        /// The handler for HtmlToPdfElement.PrepareRenderPdfPageEvent event where you can set the visibility of header and footer
        /// in each page or you can add a custom header or footer in a page
        /// </summary>
        /// <param name="eventParams">The event parameter containin the PDF page to customize before rendering</param>
        void htmlToPdfElement_PrepareRenderPdfPageEvent(PrepareRenderPdfPageParams eventParams)
        {
            // Set the header visibility in first, odd and even pages
            if (formCollection["addHeaderCheckBox"].Count > 0)
            {
                if (eventParams.PageNumber == 1)
                    eventParams.Page.ShowHeader = formCollection["showHeaderInFirstPageCheckBox"].Count > 0;
                else if ((eventParams.PageNumber % 2) == 0 && !(formCollection["showHeaderInEvenPagesCheckBox"].Count > 0))
                    eventParams.Page.ShowHeader = false;
                else if ((eventParams.PageNumber % 2) == 1 && !(formCollection["showHeaderInOddPagesCheckBox"].Count > 0))
                    eventParams.Page.ShowHeader = false;
            }

            // Set the footer visibility in first, odd and even pages
            if (formCollection["addFooterCheckBox"].Count > 0)
            {
                if (eventParams.PageNumber == 1)
                    eventParams.Page.ShowFooter = formCollection["showFooterInFirstPageCheckBox"].Count > 0;
                else if ((eventParams.PageNumber % 2) == 0 && !(formCollection["showFooterInEvenPagesCheckBox"].Count > 0))
                    eventParams.Page.ShowFooter = false;
                else if ((eventParams.PageNumber % 2) == 1 && !(formCollection["showFooterInOddPagesCheckBox"].Count > 0))
                    eventParams.Page.ShowFooter = false;
            }
        }

        /// <summary>
        /// Add a header to document
        /// </summary>
        /// <param name="pdfDocument">The PDF document object</param>
        /// <param name="drawHeaderLine">A flag indicating if a line should be drawn at the bottom of the header</param>
        private void AddHeader(Document pdfDocument, bool drawHeaderLine)
        {
            string headerHtmlUrl = m_hostingEnvironment.ContentRootPath + "/wwwroot" + "/DemoAppFiles/Input/HTML_Files/Header_HTML.html";

            // Create the document footer template
            pdfDocument.AddHeaderTemplate(60);

            // Create a HTML element to be added in header
            HtmlToPdfElement headerHtml = new HtmlToPdfElement(headerHtmlUrl);

            // Set the HTML element to fit the container height
            headerHtml.FitHeight = true;

            // Add HTML element to header
            pdfDocument.Header.AddElement(headerHtml);

            if (drawHeaderLine)
            {
                float headerWidth = pdfDocument.Header.Width;
                float headerHeight = pdfDocument.Header.Height;

                // Create a line element for the bottom of the header
                LineElement headerLine = new LineElement(0, headerHeight - 1, headerWidth, headerHeight - 1);

                // Set line color
                headerLine.ForeColor = Color.Gray;

                // Add line element to the bottom of the header
                pdfDocument.Header.AddElement(headerLine);
            }
        }

        /// <summary>
        /// Add a footer to document
        /// </summary>
        /// <param name="pdfDocument">The PDF document object</param>
        /// <param name="addPageNumbers">A flag indicating if the page numbering is present in footer</param>
        /// <param name="drawFooterLine">A flag indicating if a line should be drawn at the top of the footer</param>
        private void AddFooter(Document pdfDocument, bool addPageNumbers, bool drawFooterLine)
        {
            string footerHtmlUrl = m_hostingEnvironment.ContentRootPath + "/wwwroot" + "/DemoAppFiles/Input/HTML_Files/Footer_HTML.html";

            // Create the document footer template
            pdfDocument.AddFooterTemplate(60);

            // Set footer background color
            RectangleElement backColorRectangle = new RectangleElement(0, 0, pdfDocument.Footer.Width, pdfDocument.Footer.Height);
            backColorRectangle.BackColor = Color.WhiteSmoke;
            pdfDocument.Footer.AddElement(backColorRectangle);

            // Create a HTML element to be added in footer
            HtmlToPdfElement footerHtml = new HtmlToPdfElement(footerHtmlUrl);

            // Set the HTML element to fit the container height
            footerHtml.FitHeight = true;

            // Add HTML element to footer
            pdfDocument.Footer.AddElement(footerHtml);

            // Add page numbering
            if (addPageNumbers)
            {
                // Create a text element with page numbering place holders &p; and & P;
                TextElement footerText = new TextElement(0, 30, "Page &p; of &P;  ",
                    new System.Drawing.Font(new System.Drawing.FontFamily("Times New Roman"), 10, System.Drawing.GraphicsUnit.Point));

                // Align the text at the right of the footer
                footerText.TextAlign = HorizontalTextAlign.Right;

                // Set page numbering text color
                footerText.ForeColor = Color.Navy;

                // Embed the text element font in PDF
                footerText.EmbedSysFont = true;

                // Add the text element to footer
                pdfDocument.Footer.AddElement(footerText);
            }

            if (drawFooterLine)
            {
                float footerWidth = pdfDocument.Footer.Width;

                // Create a line element for the top of the footer
                LineElement footerLine = new LineElement(0, 0, footerWidth, 0);

                // Set line color
                footerLine.ForeColor = Color.Gray;

                // Add line element to the bottom of the footer
                pdfDocument.Footer.AddElement(footerLine);
            }
        }
    }
}