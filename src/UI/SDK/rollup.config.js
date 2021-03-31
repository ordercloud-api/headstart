import typescript from 'rollup-plugin-typescript2'
import pkg from './package.json'
import {
    terser
} from 'rollup-plugin-terser';

export default [{
        input: 'src/index.ts',
        output: {
            file: pkg.module,
            format: 'esm'
        },
        external: ['axios'],
        plugins: [
            typescript({
                typescript: require('typescript'),
            }),
        ],
    },
    {
        input: 'src/index.ts',
        output: {
            file: pkg.main,
            format: 'umd',
            name: 'OrderCloud',
            globals: {
                axios: 'axios'
            },
            esModule: false
        },
        plugins: [
            typescript({
                typescript: require('typescript'),
            }),
            terser()
        ],
        external: ['axios']
    }
]