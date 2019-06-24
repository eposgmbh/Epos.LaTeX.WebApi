#!/bin/bash
docker-compose -f docker-compose.yaml build
gcloud auth configure-docker
docker push gcr.io/eposgmbh/latex-service:latest
