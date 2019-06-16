#!/bin/bash
docker-compose -f docker-compose.yaml up --build "$@"
# Optional kann z.B. -d (fuer detached) uebergeben werden.
