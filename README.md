## Servidor Local (Windows):
Primero deberás clonar el repositorio de Ubiq, luego proseguir con lo siguiente:
~~~
cd \...\ubiq\Node
~~~

* Comandos:
~~~
npx ts-node app.ts
npm install --save-dev ts-node typescript @types/node
npx ts-node app.ts
npx ts-node --loader ts-node/esm app.ts
npx tsc app.ts --outDir dist
node dist/app.js
npx ts-node --loader ts-node/esm app.ts
npm install -g ts-node typescript
node --loader ts-node/esm app.ts
node --loader ts-node/esm app.ts
node --loader ts-node/esm app.ts
~~~
