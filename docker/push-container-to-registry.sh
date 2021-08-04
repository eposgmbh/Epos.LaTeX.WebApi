#!/bin/bash
docker login ecr.eposgmbh.eu
docker-compose -f docker-compose.yaml build && \
docker push ecr.eposgmbh.eu/latex-service:latest
