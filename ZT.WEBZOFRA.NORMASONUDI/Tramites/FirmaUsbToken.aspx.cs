using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Security;

public partial class FirmaUsbToken : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null)
        {
            Response.Redirect("~/Login.aspx");
            return;
        }

        if (string.IsNullOrEmpty(Request.QueryString["id"]))
        {
            Response.Redirect("~/Tramites/Bandeja.aspx");
            return;
        }

        if (!IsPostBack)
        {
            int idDoc = Convert.ToInt32(Request.QueryString["id"]);
            ViewState["IDDocumento"] = idDoc;

            if (!VerificarHabilitacion(idDoc))
            {
                LblError.Text = "No es su turno de firmar o el documento no requiere su firma en este momento.";
                LblError.Visible = true;
                PnlFirma.Enabled = false;
                return;
            }

            CargarInfoDocumento(idDoc);
            CargarCertificados();
        }
    }

    protected void LnkVolver_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Tramites/Detalle.aspx?id=" + ViewState["IDDocumento"]);
    }

    protected void BtnIrBandeja_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Tramites/Bandeja.aspx");
    }

    protected void BtnRefrescar_Click(object sender, EventArgs e)
    {
        CargarCertificados();
    }

    private void CargarCertificados()
    {
        DdlCertificados.Items.Clear();
        DdlCertificados.Items.Add(new ListItem("-- Seleccione un certificado --", ""));

        int count = 0;
        foreach (X509Certificate2 cert in EnumerarCertificadosDisponibles())
        {
            string titular = cert.GetNameInfo(X509NameType.SimpleName, false);
            string emisor = cert.GetNameInfo(X509NameType.SimpleName, true);
            string vence = cert.NotAfter.ToString("dd/MM/yyyy");
            string label = string.Format("{0} - {1} - vence {2}", titular, emisor, vence);
            if (!cert.HasPrivateKey)
            {
                label += " (sin clave privada detectable)";
            }

            DdlCertificados.Items.Add(new ListItem(label, cert.Thumbprint));
            count++;
        }

        LblCertInfo.Text = count == 0
            ? "No se encontraron certificados con clave privada. Inserte el DNIe o Token y haga clic en Refrescar."
            : string.Format("{0} certificado(s) disponible(s). Verifique que el dispositivo este insertado.", count);
    }

    private IEnumerable<X509Certificate2> EnumerarCertificadosDisponibles()
    {
        var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var encontrados = new List<X509Certificate2>();
        var stores = new List<Tuple<StoreName, StoreLocation>>
        {
            new Tuple<StoreName, StoreLocation>(StoreName.My, StoreLocation.CurrentUser),
            new Tuple<StoreName, StoreLocation>(StoreName.My, StoreLocation.LocalMachine)
        };

        foreach (var item in stores)
        {
            foreach (var cert in EnumerarCertificadosDeStore(new X509Store(item.Item1, item.Item2)))
            {
                if (!EsCandidatoFirma(cert)) continue;
                if (EsCertificadoDnie(cert)) continue;
                string thumbprint = NormalizarThumbprint(cert.Thumbprint);
                if (string.IsNullOrWhiteSpace(thumbprint)) continue;
                if (usados.Contains(thumbprint)) continue;
                usados.Add(thumbprint);
                encontrados.Add(cert);
            }
        }

        foreach (var cert in EnumerarCertificadosDeStore(new X509Store("SmartCard", StoreLocation.CurrentUser)))
        {
            if (!EsCandidatoFirma(cert)) continue;
            if (EsCertificadoDnie(cert)) continue;
            string thumbprint = NormalizarThumbprint(cert.Thumbprint);
            if (string.IsNullOrWhiteSpace(thumbprint)) continue;
            if (usados.Contains(thumbprint)) continue;
            usados.Add(thumbprint);
            encontrados.Add(cert);
        }

        encontrados.Sort((a, b) =>
        {
            int hasKey = b.HasPrivateKey.CompareTo(a.HasPrivateKey);
            if (hasKey != 0) return hasKey;
            return b.NotAfter.CompareTo(a.NotAfter);
        });

        return encontrados;
    }

    private bool EsCandidatoFirma(X509Certificate2 cert)
    {
        if (cert == null) return false;
        DateTime ahora = DateTime.Now;
        if (cert.NotBefore > ahora || cert.NotAfter < ahora) return false;

        var eku = cert.Extensions["2.5.29.37"] as X509EnhancedKeyUsageExtension;
        if (eku == null || eku.EnhancedKeyUsages == null || eku.EnhancedKeyUsages.Count == 0)
        {
            return true;
        }

        string[] allowedOids =
        {
            "1.3.6.1.5.5.7.3.2",  // Client Authentication
            "1.3.6.1.5.5.7.3.3",  // Code Signing
            "1.3.6.1.5.5.7.3.4",  // Email Protection
            "1.3.6.1.4.1.311.10.3.12" // Document Signing (Microsoft)
        };

        foreach (Oid oid in eku.EnhancedKeyUsages)
        {
            foreach (string allowed in allowedOids)
            {
                if (oid.Value == allowed)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerable<X509Certificate2> EnumerarCertificadosDeStore(X509Store store)
    {
        using (store)
        {
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            }
            catch
            {
                yield break;
            }

            foreach (X509Certificate2 cert in store.Certificates)
            {
                yield return cert;
            }
        }
    }

    private bool VerificarHabilitacion(int idDoc)
    {
        string loginActual = Session["strUsuario"].ToString();
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        bool habilitado = false;

        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"SELECT IDDocumentoFirmante
                             FROM FIR_DocumentoFirmante
                             WHERE IDDocumento = @IDDocumento
                               AND LoginUsuario = @LoginUsuario
                               AND Habilitado = 1
                               AND CodigoEstadoFirma = 'PEN'";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    ViewState["IDDocumentoFirmante"] = Convert.ToInt32(result);
                    habilitado = true;
                }
            }
        }
        return habilitado;
    }

    private void CargarInfoDocumento(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT d.CodigoDocumento, d.Asunto,
                       ISNULL(
                           (SELECT TOP 1 RutaFirmaParcial
                            FROM FIR_DocumentoFirmante
                            WHERE IDDocumento = d.IDDocumento AND CodigoEstadoFirma = 'FIR'
                            ORDER BY OrdenFirma DESC),
                           d.RutaArchivoPDF
                       ) AS RutaArchivoPDF
                FROM FIR_Documento d
                WHERE d.IDDocumento = @IDDocumento";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        LblCodigoDoc.Text = dr["CodigoDocumento"].ToString();
                        LblAsunto.Text = dr["Asunto"].ToString();

                        string ruta = dr["RutaArchivoPDF"].ToString();
                        if (ruta.StartsWith("ARC::"))
                        {
                            ViewState["IDArchivoPDF"] = ruta.Replace("ARC::", "");
                            HfPdfId.Value = ViewState["IDArchivoPDF"].ToString();
                        }
                        else
                        {
                            ViewState["IDArchivoPDF"] = ruta;
                            HfPdfId.Value = ViewState["IDArchivoPDF"].ToString();
                        }
                        SetHiddenValue("HfSigPage", "1");

                        try
                        {
                            int idArchivo = Convert.ToInt32(ViewState["IDArchivoPDF"]);
                            byte[] pdfBytes = ObtenerPdfBase();
                            HfSigRects.Value = BuildSignatureRectsJson(pdfBytes);
                        }
                        catch
                        {
                            HfSigRects.Value = "[]";
                        }
                    }
                }
            }
        }
    }

    protected void BtnFirmarUsb_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        PnlResultado.Visible = false;

        try
        {
            if (string.IsNullOrWhiteSpace(DdlCertificados.SelectedValue))
                throw new Exception("Debe seleccionar un certificado de la lista.");

            int idDoc = (int)ViewState["IDDocumento"];

            if (!VerificarHabilitacion(idDoc))
                throw new Exception("Sesion expirada o ya no tiene permisos para firmar este documento.");

            X509Certificate2 selectedCert = ObtenerCertificado(DdlCertificados.SelectedValue);
            if (selectedCert == null)
                throw new Exception("No se pudo obtener el certificado seleccionado. Verifique que el dispositivo este conectado y haga clic en Refrescar.");

            byte[] pdfBytes = ObtenerPdfBase();
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new Exception("No se pudo obtener el PDF del documento.");

            var placement = ObtenerPosicionFirma();
            
            byte[] firmadoBytes = null;
            Exception threadEx = null;
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                try
                {
                    firmadoBytes = FirmarPdfConCertificado(pdfBytes, selectedCert, placement);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null)
            {
                throw threadEx;
            }

            if (firmadoBytes == null || firmadoBytes.Length == 0)
                throw new Exception("No se pudo firmar el PDF.");

            RegistrarFirma(firmadoBytes, selectedCert);

            PnlFirma.Visible = false;
            PnlExito.Visible = true;
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al firmar: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private byte[] ObtenerPdfBase()
    {
        if (ViewState["IDArchivoPDF"] == null) return null;
        int idArchivo = Convert.ToInt32(ViewState["IDArchivoPDF"]);

        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString))
        using (SqlCommand cmd = new SqlCommand("SELECT Contenido FROM ARC_DocumentoArchivo WHERE IDArchivo=@ID", conn))
        {
            cmd.Parameters.AddWithValue("@ID", idArchivo);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? (byte[])result : null;
        }
    }

    private struct FirmaPlacement
    {
        public int Page;
        public float X;
        public float Y;
        public float W;
        public float H;
        public bool Vertical;
    }

    private FirmaPlacement ObtenerPosicionFirma()
    {
        float x = ParseFloat(GetHiddenValue("HfSigX"), 40f);
        float y = ParseFloat(GetHiddenValue("HfSigY"), 40f);
        float w = ParseFloat(GetHiddenValue("HfSigW"), 200f);
        float h = ParseFloat(GetHiddenValue("HfSigH"), 60f);
        int page = ParseInt(GetHiddenValue("HfSigPage"), 1);
        string orient = GetHiddenValue("HfSigOrient");
        bool vertical = string.Equals(orient, "V", StringComparison.OrdinalIgnoreCase);

        if (w <= 0 || h <= 0)
        {
            w = 200f;
            h = 60f;
        }

        return new FirmaPlacement { Page = page, X = x, Y = y, W = w, H = h, Vertical = vertical };
    }

    private float ParseFloat(string value, float fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        float result;
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }
        return fallback;
    }

    private int ParseInt(string value, int fallback)
    {
        int result;
        if (int.TryParse(value, out result))
        {
            return result;
        }
        return fallback;
    }

    private string GetHiddenValue(string id)
    {
        string posted = Request.Form[id];
        if (!string.IsNullOrWhiteSpace(posted)) return posted;

        HiddenField field = FindHiddenField(Page, id);
        if (field == null) return string.Empty;

        string postedUnique = Request.Form[field.UniqueID];
        if (!string.IsNullOrWhiteSpace(postedUnique)) return postedUnique;

        return field.Value;
    }

    private HiddenField FindHiddenField(Control root, string id)
    {
        foreach (Control control in root.Controls)
        {
            if (control is HiddenField hidden && hidden.ID == id)
            {
                return hidden;
            }

            HiddenField found = FindHiddenField(control, id);
            if (found != null) return found;
        }

        return null;
    }

    private void SetHiddenValue(string id, string value)
    {
        HiddenField field = FindControl(id) as HiddenField;
        if (field == null) return;
        if (string.IsNullOrWhiteSpace(field.Value))
        {
            field.Value = value;
        }
    }

    private byte[] FirmarPdfConCertificado(byte[] inputBytes, X509Certificate2 selectedCert, FirmaPlacement placement)
    {
        string titular = selectedCert.GetNameInfo(X509NameType.SimpleName, false);
        using (PdfReader reader = new PdfReader(inputBytes))
        using (MemoryStream outputStream = new MemoryStream())
        {
            if (HasSignatureCollision(reader, placement))
            {
                throw new Exception("La posicion seleccionada se superpone con una firma existente.");
            }

            PdfStamper stamper = PdfStamper.CreateSignature(reader, outputStream, '\0', null, true);

            PdfSignatureAppearance appearance = stamper.SignatureAppearance;
            appearance.Reason = "Firma Oficial ZOFRATACNA";
            appearance.Location = "Tacna, Peru";
            appearance.SignatureCreator = titular;

            int pageNumber = placement.Page <= 0 ? 1 : placement.Page;
            if (pageNumber > reader.NumberOfPages) pageNumber = 1;

            float x = placement.X;
            float y = placement.Y;
            float w = placement.W;
            float h = placement.H;

            appearance.SetVisibleSignature(
                new iTextSharp.text.Rectangle(x, y, x + w, y + h), pageNumber, "Firma_" + Guid.NewGuid().ToString("N"));

            if (placement.Vertical)
            {
                appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC;
                appearance.SignatureGraphic = BuildSignatureGraphic(BuildSignatureText(titular), w, h, false);
            }

            IExternalSignature externalSignature = new Bit4idExplicitSignature(selectedCert, "SHA-256");

            MakeSignature.SignDetached(
                appearance,
                externalSignature,
                new Org.BouncyCastle.X509.X509Certificate[]
                {
                    DotNetUtilities.FromX509Certificate(selectedCert)
                },
                null, null, null, 0, CryptoStandard.CMS);

            stamper.Close();

            return outputStream.ToArray();
        }
    }

    private string BuildSignatureRectsJson(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0) return "[]";

        var rects = new List<Dictionary<string, object>>();
        using (var reader = new PdfReader(pdfBytes))
        {
            var fields = reader.AcroFields;
            var sigNames = fields.GetSignatureNames();
            foreach (string name in sigNames)
            {
                var positions = fields.GetFieldPositions(name);
                if (positions == null) continue;
                foreach (var pos in positions)
                {
                    rects.Add(new Dictionary<string, object>
                    {
                        { "page", pos.page },
                        { "x", pos.position.Left },
                        { "y", pos.position.Bottom },
                        { "w", pos.position.Width },
                        { "h", pos.position.Height }
                    });
                }
            }
        }

        return new JavaScriptSerializer().Serialize(rects);
    }

    private bool HasSignatureCollision(PdfReader reader, FirmaPlacement placement)
    {
        var fields = reader.AcroFields;
        var sigNames = fields.GetSignatureNames();
        if (sigNames == null || sigNames.Count == 0) return false;

        int pageNumber = placement.Page <= 0 ? 1 : placement.Page;
        if (pageNumber > reader.NumberOfPages) pageNumber = 1;

        float x1 = placement.X;
        float y1 = placement.Y;
        float x2 = placement.X + placement.W;
        float y2 = placement.Y + placement.H;

        foreach (string name in sigNames)
        {
            var positions = fields.GetFieldPositions(name);
            if (positions == null) continue;
            foreach (var pos in positions)
            {
                if (pos.page != pageNumber) continue;
                float llx = pos.position.Left;
                float lly = pos.position.Bottom;
                float urx = pos.position.Right;
                float ury = pos.position.Top;
                bool overlap = x1 < urx && x2 > llx && y1 < ury && y2 > lly;
                if (overlap) return true;
            }
        }

        return false;
    }

    private void RegistrarFirma(byte[] contenidoFirmado, X509Certificate2 selectedCert)
    {
        int idDoc = (int)ViewState["IDDocumento"];
        int idDocFirmante = (int)ViewState["IDDocumentoFirmante"];
        string loginActual = Session["strUsuario"].ToString();
        string ipEquipo = Request.UserHostAddress;

        int idArchivoFirmado = 0;
        string codigoDocumento = LblCodigoDoc.Text;
        string nombreArchivoG = codigoDocumento + "_firmado_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";

        string connArchivosStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connArchivosStr))
        {
            using (SqlCommand cmd = new SqlCommand("ARC_I_GuardarArchivo_OUT", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Contenido", contenidoFirmado);
                cmd.Parameters.AddWithValue("@NombreOriginal", nombreArchivoG);
                cmd.Parameters.AddWithValue("@TipoArchivo", "PDF_FIRMADO");
                cmd.Parameters.AddWithValue("@IDUsuarioCreador", loginActual);

                SqlParameter pout = new SqlParameter("@IDArchivo", SqlDbType.Int);
                pout.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(pout);

                conn.Open();
                cmd.ExecuteNonQuery();
                idArchivoFirmado = Convert.ToInt32(pout.Value);
            }
        }

        string hashStr = "";
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(contenidoFirmado);
            hashStr = BitConverter.ToString(hash).Replace("-", "");
        }

        bool procesoCompleto = false;
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            using (SqlCommand cmd = new SqlCommand("FIR_X_RegistrarFirma_OUT", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                cmd.Parameters.AddWithValue("@IDDocumentoFirmante", idDocFirmante);
                cmd.Parameters.AddWithValue("@HashFirma", hashStr);
                cmd.Parameters.AddWithValue("@CertificadoSN", selectedCert.SerialNumber ?? "USB");
                cmd.Parameters.AddWithValue("@CertificadoDNI", DBNull.Value);
                cmd.Parameters.AddWithValue("@MotivoFirma", string.Empty);
                cmd.Parameters.AddWithValue("@FirmaValida", 1);
                cmd.Parameters.AddWithValue("@RespuestaMotor", "Firma USB-Token" );
                cmd.Parameters.AddWithValue("@RutaPDFFirmado", "ARC::" + idArchivoFirmado);
                cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                cmd.Parameters.AddWithValue("@IDEquipo", ipEquipo);

                SqlParameter pOut = new SqlParameter("@ProcesoCompleto", SqlDbType.Bit);
                pOut.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(pOut);

                conn.Open();
                cmd.ExecuteNonQuery();
                procesoCompleto = Convert.ToBoolean(pOut.Value);
            }

            EnviarNotificaciones(conn, idDoc, procesoCompleto);
        }

        LblMensajeExito.Text = procesoCompleto
            ? "El documento ha sido firmado por todos los participantes y se encuentra completado."
            : "Su firma ha sido registrada correctamente. El documento ha sido notificado al siguiente firmante.";
    }

    private iTextSharp.text.Image BuildSignatureGraphic(string text, float maxWidth, float maxHeight, bool vertical)
    {
        const int pad = 4;
        const float scale = 2.5f;
        const float minFont = 4f;
        const float maxFont = 12f;

        int bmpW = Math.Max(1, (int)Math.Ceiling(maxWidth * scale));
        int bmpH = Math.Max(1, (int)Math.Ceiling(maxHeight * scale));
        float scaledPad = pad * scale;

        float drawW = vertical ? bmpH : bmpW;
        float drawH = vertical ? bmpW : bmpH;

        using (var bmp = new Bitmap(bmpW, bmpH))
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.Transparent);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            float fitFontSize = FitFontSize(g, text,
                drawW - (scaledPad * 2),
                drawH - (scaledPad * 2),
                minFont * scale, maxFont * scale);

            using (var font = new Font("Arial", fitFontSize, FontStyle.Regular, GraphicsUnit.Point))
            {
                var format = new StringFormat
                {
                    Alignment = vertical ? StringAlignment.Near : StringAlignment.Near,
                    LineAlignment = vertical ? StringAlignment.Near : StringAlignment.Center,
                    Trimming = StringTrimming.None,
                    FormatFlags = StringFormatFlags.LineLimit
                };

                if (vertical)
                {
                    g.TranslateTransform(bmp.Width / 2f, bmp.Height / 2f);
                    g.RotateTransform(-90f);
                    g.TranslateTransform(-bmpH / 2f, -bmpW / 2f);
                }

                g.DrawString(text, font, Brushes.Black,
                    new RectangleF(scaledPad, scaledPad, drawW - (scaledPad * 2), drawH - (scaledPad * 2)), format);
            }

            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                var img = iTextSharp.text.Image.GetInstance(ms.ToArray());
                img.ScaleAbsolute(maxWidth, maxHeight);
                return img;
            }
        }
    }

    private string BuildSignatureText(string titular)
    {
        string fecha = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss zzz");
        return "Digitally signed by " + titular + "\n" +
               "Date: " + fecha + "\n" +
               "Reason: Firma Oficial ZOFRATACNA\n" +
               "Location: Tacna, Peru";
    }

    private float FitFontSize(Graphics g, string text, float maxWidth, float maxHeight, float min, float max)
    {
        float best = min;
        float low = min;
        float high = max;

        for (int i = 0; i < 12; i++)
        {
            float mid = (low + high) / 2f;
            using (var font = new Font("Arial", mid, FontStyle.Regular, GraphicsUnit.Point))
            {
                SizeF size = g.MeasureString(text, font, (int)Math.Max(1, maxWidth));
                if (size.Width <= maxWidth && size.Height <= maxHeight)
                {
                    best = mid;
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }
        }

        return best;
    }


    private X509Certificate2 ObtenerCertificado(string thumbprint)
    {
        string target = NormalizarThumbprint(thumbprint);
        if (string.IsNullOrWhiteSpace(target)) return null;

        foreach (X509Certificate2 cert in EnumerarCertificadosDisponibles())
        {
            string current = NormalizarThumbprint(cert.Thumbprint);
            if (string.Equals(current, target, StringComparison.OrdinalIgnoreCase))
            {
                return cert;
            }
        }

        return null;
    }

    private string NormalizarThumbprint(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return value.Replace(" ", string.Empty).Trim().ToUpperInvariant();
    }

    private void EnviarNotificaciones(SqlConnection conn, int idDoc, bool completo)
    {
        string codigoDoc = LblCodigoDoc.Text;
        string asunto = LblAsunto.Text;

        if (completo)
        {
            EnviarCorreoInvolucrados(idDoc,
                "Tramite completado - " + codigoDoc,
                "<p>El siguiente documento ha sido firmado completamente:</p>" +
                "<div style='background:#f9f9f9;border-left:4px solid #1a5c38;padding:16px;margin:16px 0;'>" +
                "  <b>Codigo:</b> " + codigoDoc + "<br/>" +
                "  <b>Asunto:</b> " + asunto + "<br/>" +
                "  <b>Estado:</b> Firmado Completamente" +
                "</div>" +
                "<p>El tramite ha concluido exitosamente. Puede descargar el documento final en el sistema.</p>");
        }
        else
        {
            string querySiguiente = @"SELECT TOP 1 CorreoFirmante, NombreFirmante
                                      FROM FIR_DocumentoFirmante
                                      WHERE IDDocumento = @IDD AND Habilitado = 1 AND CodigoEstadoFirma = 'PEN'";
            using (SqlCommand cmd = new SqlCommand(querySiguiente, conn))
            {
                cmd.Parameters.AddWithValue("@IDD", idDoc);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string correo = dr["CorreoFirmante"].ToString();
                        if (!string.IsNullOrWhiteSpace(correo))
                        {
                            EnviarMail(conn, correo, "Tramite pendiente de firma: " + codigoDoc,
                                "<p>El documento <b>" + codigoDoc + "</b> ya fue firmado por el participante anterior y se encuentra disponible para su firma.</p>");
                        }
                    }
                }
            }
        }
    }

    private string PlantillaCorreo(string contenido)
    {
        return @"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5;'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:20px;'>
        <table width='600' cellpadding='0' cellspacing='0'
               style='background:#fff;border-radius:8px;overflow:hidden;'>
          <tr>
            <td style='background:#1a5c38;padding:24px;text-align:center;'>
              <h1 style='color:#fff;margin:0;font-size:18px;letter-spacing:2px;'>
                SISTEMA DE FIRMA ZOFRATACNA
              </h1>
            </td>
          </tr>
          <tr>
            <td style='padding:32px;color:#333;'>
              <p>Estimado(a),</p>
              " + contenido + @"
              <br/>
              <p>Atentamente,</p>
              <p><b>Oficina de Tecnologias de la Informacion</b><br/>
              ZOFRATACNA</p>
            </td>
          </tr>
          <tr>
            <td style='background:#222;padding:16px;text-align:center;'>
              <p style='color:#aaa;margin:0;font-size:12px;'>
                Este es un correo automatico del Sistema Firmador ZOFRATACNA.
                Por favor no responda este mensaje.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    private void EnviarCorreoInvolucrados(int idDocumento, string asunto, string mensaje)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            System.Collections.Generic.List<string> correos = new System.Collections.Generic.List<string>();
            using (SqlConnection connInfo = new SqlConnection(connStr))
            {
                connInfo.Open();
                string query = @"SELECT DISTINCT e.Email, e.NombreCompleto
                                 FROM FIR_VW_EmpleadosActivos e
                                 WHERE e.LoginUsuario IN (
                                     SELECT LoginRegistrador FROM FIR_Documento WHERE IDDocumento = @IDDocumento
                                     UNION
                                     SELECT LoginUsuario FROM FIR_DocumentoFirmante WHERE IDDocumento = @IDDocumento
                                 )
                                 AND (e.Email NOT LIKE '%zofratacna.com.pe' OR e.Email LIKE '%zofratacna.com.pe')";

                using (SqlCommand cmd = new SqlCommand(query, connInfo))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDocumento);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string email = dr["Email"].ToString();
                            if (!string.IsNullOrWhiteSpace(email))
                            {
                                correos.Add(email);
                            }
                        }
                    }
                }

                string mensajeHTML = PlantillaCorreo(mensaje);

                foreach (string email in correos)
                {
                    try
                    {
                        using (SqlCommand cmdMail = new SqlCommand("GEN_X_EnviarMail", connInfo))
                        {
                            cmdMail.CommandType = CommandType.StoredProcedure;
                            cmdMail.Parameters.AddWithValue("@Para", email);
                            cmdMail.Parameters.AddWithValue("@Asunto", asunto);
                            cmdMail.Parameters.AddWithValue("@Mensaje", mensajeHTML);
                            cmdMail.ExecuteNonQuery();
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    private void EnviarMail(SqlConnection conn, string para, string asunto, string body)
    {
        try
        {
            string htmlBody = PlantillaCorreo(body);
            using (SqlCommand cmd = new SqlCommand("GEN_X_EnviarMail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Para", para);
                cmd.Parameters.AddWithValue("@Asunto", asunto);
                cmd.Parameters.AddWithValue("@Mensaje", htmlBody);
                cmd.ExecuteNonQuery();
            }
        }
        catch { }
    }

    [System.Runtime.InteropServices.DllImport("crypt32.dll", SetLastError = true)]
    private static extern bool CertGetCertificateContextProperty(
        IntPtr pCertContext,
        uint dwPropId,
        IntPtr pvData,
        ref uint pcbData);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private struct CRYPT_KEY_PROV_INFO
    {
        public string pwszContainerName;
        public string pwszProvName;
        public uint dwProvType;
        public uint dwFlags;
        public uint cProvParam;
        public IntPtr rgProvParam;
        public uint dwKeySpec;
    }

    private static string ObtenerNombreProviderPropiedad(X509Certificate2 cert)
    {
        if (cert == null || cert.Handle == IntPtr.Zero) return null;
        uint pcbData = 0;
        const uint CERT_KEY_PROV_INFO_PROP_ID = 2;

        if (!CertGetCertificateContextProperty(cert.Handle, CERT_KEY_PROV_INFO_PROP_ID, IntPtr.Zero, ref pcbData))
        {
            return null;
        }

        IntPtr pvData = System.Runtime.InteropServices.Marshal.AllocHGlobal((int)pcbData);
        try
        {
            if (CertGetCertificateContextProperty(cert.Handle, CERT_KEY_PROV_INFO_PROP_ID, pvData, ref pcbData))
            {
                var provInfo = (CRYPT_KEY_PROV_INFO)System.Runtime.InteropServices.Marshal.PtrToStructure(pvData, typeof(CRYPT_KEY_PROV_INFO));
                return provInfo.pwszProvName;
            }
        }
        catch {}
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pvData);
        }
        return null;
    }

    private bool EsCertificadoDnie(X509Certificate2 cert)
    {
        if (cert == null) return false;

        // Si el emisor o issuerDN contiene RENIEC o ECEP, es DNIe
        string emisor = cert.GetNameInfo(X509NameType.SimpleName, true);
        string issuerDN = cert.Issuer;
        if ((emisor != null && (emisor.IndexOf("RENIEC", StringComparison.OrdinalIgnoreCase) >= 0 || emisor.IndexOf("ECEP", StringComparison.OrdinalIgnoreCase) >= 0)) ||
            (issuerDN != null && (issuerDN.IndexOf("RENIEC", StringComparison.OrdinalIgnoreCase) >= 0 || issuerDN.IndexOf("ECEP", StringComparison.OrdinalIgnoreCase) >= 0)))
        {
            return true;
        }

        // Si el provider de la tarjeta es IDProtect
        string providerName = ObtenerNombreProviderPropiedad(cert);
        if (!string.IsNullOrEmpty(providerName))
        {
            if (providerName.IndexOf("IDProtect", StringComparison.OrdinalIgnoreCase) >= 0 ||
                providerName.IndexOf("EnterSafe", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private class Bit4idExplicitSignature : IExternalSignature
    {
        private readonly X509Certificate2 _cert;
        private readonly string _hashAlgorithm;

        public Bit4idExplicitSignature(X509Certificate2 cert, string hashAlgorithm)
        {
            _cert = cert;
            _hashAlgorithm = hashAlgorithm;
        }

        public string GetHashAlgorithm()
        {
            return _hashAlgorithm;
        }

        public string GetEncryptionAlgorithm()
        {
            return "RSA";
        }

        public byte[] Sign(byte[] message)
        {
            string keyName = null;
            try
            {
                using (RSA rsaDefault = _cert.GetRSAPrivateKey())
                {
                    if (rsaDefault is RSACng rsaCng)
                    {
                        keyName = rsaCng.Key.KeyName;
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(keyName))
            {
                try
                {
                    if (_cert.PrivateKey is RSACryptoServiceProvider csp && csp.CspKeyContainerInfo != null)
                    {
                        keyName = csp.CspKeyContainerInfo.KeyContainerName;
                    }
                }
                catch { }
            }

            if (string.IsNullOrEmpty(keyName))
            {
                // Fallback directo si no podemos encontrar el nombre del contenedor
                try
                {
                    using (RSA rsaDefault = _cert.GetRSAPrivateKey())
                    {
                        if (rsaDefault != null)
                        {
                            return rsaDefault.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                        }
                    }
                }
                catch { }
                throw new Exception("No se pudo obtener el nombre del contenedor de la clave privada para Bit4id KSP.");
            }

            try
            {
                using (CngKey cngKey = CngKey.Open(keyName, new CngProvider("Bit4id Key Storage Provider")))
                using (RSACng rsa = new RSACng(cngKey))
                {
                    return rsa.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                // Si falla (por ejemplo, es un certificado de software simulado o tiene otro KSP),
                // intentamos firmar con su KSP nativo antes de tirar la toalla.
                // Esto permite la simulación perfecta de tokens USB mediante certificados de software.
                try
                {
                    using (RSA rsaDefault = _cert.GetRSAPrivateKey())
                    {
                        if (rsaDefault != null)
                        {
                            return rsaDefault.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                        }
                    }
                }
                catch { }

                throw ex; // Si también falla, lanzamos la excepción original
            }
        }
    }
}
