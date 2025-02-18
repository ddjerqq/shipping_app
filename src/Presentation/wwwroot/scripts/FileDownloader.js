/**
 * Save as a file
 * @param {string} filename
 * @param {string} bytesBase64
 */
export const saveAsFile = (filename, bytesBase64) => {
  const link = document.createElement("a");
  link.download = filename;
  link.href = `data:application/octet-stream;base64,${bytesBase64}`;
  try {
    document.body.appendChild(link); // Needed for Firefox
    link.click();
  }
  catch (e) {
    console.error(e);
  }
  finally {
    document.body.removeChild(link);
  }
}