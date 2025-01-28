export const print = (selector) => {
  try {
    const element = document.querySelector(selector);
    if (!element) {
      console.error(`Element with id ${selector} not found.`);
      return;
    }

    const printContents = element.innerHTML;
    const iframe = document.createElement("iframe");
    iframe.style.position = "absolute";
    iframe.style.width = "0";
    iframe.style.height = "0";
    iframe.style.zIndex = "1000";
    iframe.style.border = "none";
    document.body.appendChild(iframe);

    const iframeWindow = iframe.contentWindow;
    if (!iframeWindow) {
      console.error("Failed to get iframe content window.");
      document.body.removeChild(iframe);
      return;
    }

    const doc = iframeWindow.document;
    doc.open();
    doc.write("<html lang='en'><head><title>print</title>");

    // copy over styles
    const styles = document.querySelectorAll('style, link[rel="stylesheet"]');
    styles.forEach((style) => {
      if (style.tagName === "LINK") {
        doc.write(`<link rel="stylesheet" href="${style.href}">`);
      } else {
        doc.write(style.outerHTML);
      }
    });

    doc.write("</head><body>");
    doc.write(printContents);
    doc.write("</body></html>");
    doc.close();

    iframe.onload = () => {
      iframeWindow.focus();
      iframeWindow.print();
      document.body.removeChild(iframe);
      console.log("print success");
    };
  } catch (error) {
    console.error("An error occurred while trying to print:", error);
  }
};