# escape=`

ARG BASE_IMAGE

FROM ${BASE_IMAGE} as base

RUN npm config set registry http://registry.npmjs.org/ --global
RUN npm install -g @angular/cli
RUN npm install -g json

RUN apk add --no-cache git

FROM base as builder

ARG ROLE

WORKDIR /build

COPY src/UI/${ROLE}/package*.json ./

RUN npm install

COPY src/UI/${ROLE}/ .

RUN sed -i 's/useLocalMiddleware = true/useLocalMiddleware = false/g' /build/src/environments/environment.local.ts

RUN npm run build --prod

FROM nginx:1.19.8-alpine

ARG ROLE

COPY docker/build/UI/${ROLE}.sh ./entrypoint.sh

RUN apk add dos2unix
RUN dos2unix ./entrypoint.sh

RUN apk add --update nodejs nodejs-npm
RUN npm install -g json
RUN apk add --no-cache --upgrade bash

RUN ["chmod", "+x", "./entrypoint.sh"]

COPY src/UI/${ROLE}/ .

RUN sed -i 's/useLocalMiddleware = true/useLocalMiddleware = false/g' /build/src/environments/environment.local.ts

RUN npm run build --prod

FROM nginx:1.19.8-alpine

ARG ROLE

COPY docker/build/UI/${ROLE}.sh ./entrypoint.sh

RUN apk add dos2unix
RUN dos2unix ./entrypoint.sh

RUN apk add --update nodejs nodejs-npm
RUN npm install -g json
RUN apk add --no-cache --upgrade bash

RUN ["chmod", "+x", "./entrypoint.sh"]

COPY docker/build/UI/nginx/default.conf /etc/nginx/conf.d/default.conf

COPY --from=builder /build/dist /usr/share/nginx/html

COPY src/UI/${ROLE}/scripts /usr/share/nginx/html

CMD ["sh", "/entrypoint.sh"]
