FROM alpine:3.20

# gcc/g++ for compilation; jq for JSON parsing; procps for /usr/bin/time -v (peak memory)
RUN apk add --no-cache gcc g++ musl-dev jq procps

# Ensure nobody user exists at uid 65534 (Alpine standard) and nogroup at gid 65534
RUN adduser -D -H -u 65534 nobody 2>/dev/null || true \
 && addgroup -S nogroup 2>/dev/null || true

COPY compile-and-run.sh /usr/local/bin/run.sh
RUN chmod +x /usr/local/bin/run.sh

USER nobody

ENTRYPOINT ["/usr/local/bin/run.sh"]
