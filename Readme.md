# ITI SQLite Shakespeare

## Program

TODO

## Docker

### Create the docker image

The image is already configured in the Dockerfile file. You can create it with the following command:

```bash
docker build -t shakespeare .
```

Then you can create the container with:

```bash
docker create shakespeare
```

### Run the image

```bash
docker run -it --rm -v "[LocalResourcesPath]:[VolumeResourcesPath]" shakespeare "[VolumeResourcesPath]/database.sqlite" "[VolumeResourcesPath]/shakespeare.dat"
```
