<%@ WebHandler Language="C#" Class="TokenStatus" %>

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Script.Serialization;

public class TokenStatus : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

        try
        {
            var certs = new List<object>();

            foreach (X509Certificate2 cert in EnumerarCertificadosDisponibles())
            {
                string titular = cert.GetNameInfo(X509NameType.SimpleName, false);
                string emisor = cert.GetNameInfo(X509NameType.SimpleName, true);
                string vence = cert.NotAfter.ToString("dd/MM/yyyy");

                certs.Add(new
                {
                    thumbprint = NormalizarThumbprint(cert.Thumbprint),
                    titular = titular,
                    emisor = emisor,
                    vence = vence,
                    esToken = false,
                    tipo = "desconocido",
                    label = titular + " - " + emisor + " - vence " + vence
                });
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(new
            {
                ok = true,
                total = certs.Count,
                certs = certs
            }));
        }
        catch (Exception ex)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(new
            {
                ok = false,
                error = ex.Message,
                certs = new object[0]
            }));
        }
    }

    public bool IsReusable => false;

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

        foreach (Oid oid in eku.EnhancedKeyUsages)
        {
            if (oid.Value == "1.3.6.1.5.5.7.3.3" || oid.Value == "1.3.6.1.5.5.7.3.4")
            {
                return true;
            }
        }

        return false;
    }

    private string NormalizarThumbprint(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return value.Replace(" ", string.Empty).Trim().ToUpperInvariant();
    }
}
