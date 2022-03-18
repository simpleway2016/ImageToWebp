import { ImagePool } from '@squoosh/lib';
import { cpus } from 'os';
import fs from 'fs/promises';

async function main() {
    var imagePool;
    try {
        imagePool = new ImagePool(cpus().length);
        var args = process.argv.splice(2);

        const file = await fs.readFile(args[0]);


        const image = imagePool.ingestImage(file);

        const preprocessOptions = {
            //When both width and height are specified, the image resized to specified size.
            //resize: {
            //    width: 100,
            //    height: 50,
            //},
            /*
            //When either width or height is specified, the image resized to specified size keeping aspect ratio.
            resize: {
              width: 100,
            }
            */
        };
        await image.preprocess(preprocessOptions);

        const encodeOptions = {
        };

        if (args[1] == "webp") {
            encodeOptions.webp = {};
        }
        else if (args[1] == "png") {
            encodeOptions.oxipng = {};
        }
        else {
            encodeOptions.mozjpeg = {};
        }

        const result = await image.encode(encodeOptions);

        for (var p in image.encodedWith) {
            const rawEncodedImage = (await image.encodedWith[p]).binary;
            fs.writeFile(args[2], rawEncodedImage);
            break;
        }
        console.log("ok");
    } catch (e) {
        console.log(e.message);
    }
    finally {
        if (imagePool) {
            await imagePool.close();
        }
    }
    
}
main();