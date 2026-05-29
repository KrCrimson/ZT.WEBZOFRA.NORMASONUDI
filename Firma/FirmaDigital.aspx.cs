using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Web.UI.WebControls;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Security;

namespace Firma
{
    public partial class FirmaDigital : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarCertificados();
        }

        protected void btnRefrescar_Click(object sender, EventArgs e)
        {
            CargarCertificados();
        }

        private void CargarCertificados()
        {
            ddlCertificados.Items.Clear();
            ddlCertificados.Items.Add(new ListItem("-- Seleccione un certificado --", ""));

            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            int count = 0;
            foreach (X509Certificate2 cert in store.Certificates)
            {
                if (!cert.HasPrivateKey) continue;

                string titular = cert.GetNameInfo(X509NameType.SimpleName, false);
                string emisor  = cert.GetNameInfo(X509NameType.SimpleName, true);
                string vence   = cert.NotAfter.ToString("dd/MM/yyyy");
                string label   = $"{titular} — {emisor} — vence {vence}";

                ddlCertificados.Items.Add(new ListItem(label, cert.Thumbprint));
                count++;
            }

            store.Close();

            lblCertInfo.Text = count == 0
                ? "No se encontraron certificados con clave privada. Inserte el DNIe o Token y haga clic en Refrescar."
                : $"{count} certificado(s) disponible(s). Verifique que el dispositivo esté insertado.";
        }

        protected void btnFirmar_Click(object sender, EventArgs e)
        {
            try
            {
                // --- Validaciones de entrada ---
                if (string.IsNullOrWhiteSpace(ddlCertificados.SelectedValue))
                    throw new Exception("Debe seleccionar un certificado de la lista.");

                if (!fileUploadPdf.HasFile)
                    throw new Exception("Debe seleccionar un archivo PDF.");

                if (!fileUploadPdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Solo se aceptan archivos PDF.");

                // --- Obtener el certificado seleccionado ---
                X509Certificate2 selectedCert = ObtenerCertificado(ddlCertificados.SelectedValue);
                if (selectedCert == null)
                    throw new Exception("No se pudo obtener el certificado seleccionado. Verifique que el dispositivo esté conectado y haga clic en Refrescar.");

                // --- Preparar carpeta de salida ---
                string tempDir = Server.MapPath("~/Temp");
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                string fileName = "Documento_Firmado_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                string filePath = Path.Combine(tempDir, fileName);

                // --- Firmar el PDF ---
                string titular = selectedCert.GetNameInfo(X509NameType.SimpleName, false);
                byte[] inputBytes = fileUploadPdf.FileBytes;
                using (PdfReader reader = new PdfReader(inputBytes))
                using (MemoryStream outputStream = new MemoryStream())
                {
                    PdfStamper stamper = PdfStamper.CreateSignature(reader, outputStream, '\0');

                    PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                    appearance.Reason          = "Firma Oficial ZOFRATACNA";
                    appearance.Location        = "Tacna, Perú";
                    appearance.SignatureCreator = titular;

                    // Firma visible: esquina inferior izquierda de la primera página
                    appearance.SetVisibleSignature(
                        new iTextSharp.text.Rectangle(36, 36, 270, 100), 1, "Firma_Digital");

                    IExternalSignature externalSignature = new X509Certificate2Signature(selectedCert, "SHA-256");

                    MakeSignature.SignDetached(
                        appearance,
                        externalSignature,
                        new Org.BouncyCastle.X509.X509Certificate[]
                        {
                            DotNetUtilities.FromX509Certificate(selectedCert)
                        },
                        null, null, null, 0, CryptoStandard.CMS);

                    stamper.Close();

                    File.WriteAllBytes(filePath, outputStream.ToArray());
                }

                // --- Resultado exitoso ---
                lblMensaje.Text          = $"Documento firmado correctamente con: {titular}";
                lnkDescargar.NavigateUrl = "~/Temp/" + fileName;
                lnkDescargar.Visible     = true;
                lblFirmaVisible.Visible  = true;
                pnlResultado.Visible     = true;
                pnlResultado.BackColor   = System.Drawing.Color.LightGreen;
            }
            catch (Exception ex)
            {
                lblMensaje.Text       = "Error: " + ex.Message;
                lnkDescargar.Visible  = false;
                pnlResultado.Visible  = true;
                pnlResultado.BackColor = System.Drawing.Color.LightPink;
            }
        }

        // Busca el certificado por huella en el almacén de Windows
        private X509Certificate2 ObtenerCertificado(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            foreach (X509Certificate2 cert in store.Certificates)
            {
                if (cert.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    store.Close();
                    return cert;
                }
            }

            store.Close();
            return null;
        }

    }
}
