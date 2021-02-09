export const downloadBase64String = (
  base64File: string,
  fileType: string,
  fileName: string
): void => {
  const downloadLink = document.createElement('a')
  downloadLink.href = `data:${fileType};base64,${base64File}`
  downloadLink.download = fileName
  downloadLink.click()
}
