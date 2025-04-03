# Creating keys for ASP.NET is tricky, but you can do it with a simple command.

```bash
openssl ecparam -name prime256v1 -genkey -noout -out ecdsa_key.pem
openssl req -new -x509 -key ecdsa_key.pem -out ecdsa_cert.pem -days 365
openssl pkcs12 -export -out cert.pfx -in cert.pem -inkey key.pem
```

```bash
export=ASPNETCORE_KESTREL__CERTIFICATES__DEFAULT__PATH=
export=ASPNETCORE_KESTREL__CERTIFICATES__DEFAULT__PASSWORD=
```