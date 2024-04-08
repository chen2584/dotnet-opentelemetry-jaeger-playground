# dotnet-opentelemetry-playground

### Generate TLS files (Self-Certificate) for Test purpose (Not Required)
```
cfssl genkey -initca ca-csr.json | cfssljson -bare ca
cfssl gencert -ca ca.pem -ca-key ca-key.pem cert-csr.json | cfssljson -bare client
cfssl gencert -ca ca.pem -ca-key ca-key.pem cert-csr.json | cfssljson -bare server
```

### Keyclock Generate JWT Access Token Example
```
curl --location --request POST 'http://keycloak:8080/realms/opentelemetry/protocol/openid-connect/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'grant_type=client_credentials' \
--data-urlencode 'client_id=collector' \
--data-urlencode 'client_secret=ThisIsABook'
```

### Resources
Why are Prometheus histograms cumulative?
https://www.robustperception.io/why-are-prometheus-histograms-cumulative/