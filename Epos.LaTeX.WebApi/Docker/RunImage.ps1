# -rm automatically removes the container on exit.
docker run --rm --detach --publish 81:81 --name latex-webapi eposgmbh/latex-webapi
