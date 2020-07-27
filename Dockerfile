FROM microsoft/mssql-server-linux:latest

# Create work directory
RUN mkdir -p /usr/work
WORKDIR /usr/work

# Copy all scripts into working directory
COPY . /usr/work/

RUN chmod +x /usr/work/setup.sh

EXPOSE 1433

CMD /bin/bash ./entrypoint.sh