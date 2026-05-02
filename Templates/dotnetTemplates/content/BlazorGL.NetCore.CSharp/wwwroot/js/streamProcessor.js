//   streamProcessor.js
class StreamProcessor extends AudioWorkletProcessor
{
    constructor()
    {
        super();
        this.queue = [];
        this.samplePosition = 0;
        this.paused = false;
        this.pitchPlaybackRate = 1;
        this.formatSampleRate = 44100;
        this.formatChannels = 1;
        this.receivingValueForMessageType = -1;

        this.port.onmessage = (event) =>
        {
            var data = event.data;

            if (typeof data === 'number')
            {
                if (this.receivingValueForMessageType === -1)
                {
                    switch (data)
                    {
                        case 2:
                            this.queue = []; break;
                        case 3:
                            this.paused = true; break;
                        case 4:
                            this.paused = false; break;
                        case 5:
                        case 6:
                        case 7:
                            this.receivingValueForMessageType = data;
                            break;
                    }
                }
                else
                {
                    switch (this.receivingValueForMessageType)
                    {
                        case 5:
                            this.pitchPlaybackRate = data; break;
                        case 6:
                            this.formatSampleRate = data; break;
                        case 7:
                            this.formatChannels = data; break;
                    }
                    this.receivingValueForMessageType = -1;
                }
            }
            if (data instanceof Uint8Array)
            {
                const buffer = new Int16Array(data.buffer, data.byteOffset, data.length / 2);
                this.queue.push(buffer);
            }
        };
    }


    process(inputs, outputs, parameters)
    {
        const inputChannelCount = this.formatChannels;
        const formatPlaybackRate = this.formatSampleRate / sampleRate;
        const playbackRate = formatPlaybackRate * this.pitchPlaybackRate;

        const output = outputs[0];
        const outputChannelCount = output.length;
        const outputSampleCount = output[0].length;

        let written = 0;

        while (written < outputSampleCount && this.queue.length > 0 && !this.paused)
        {
            const buffer = this.queue[0];
            const samplesInBuffer = buffer.length / inputChannelCount;
            let nextSamplePosition = this.samplePosition;

            while (written < outputSampleCount && nextSamplePosition < samplesInBuffer)
            {
                let offset = Math.floor(nextSamplePosition) * inputChannelCount;
                for (let outputChannel = 0; outputChannel < outputChannelCount; outputChannel++)
                {
                    let inputChannel = outputChannel % inputChannelCount;
                    let value1 = buffer[offset + inputChannel];
                    let value2;
                    if (nextSamplePosition < samplesInBuffer - 1)
                        value2 = buffer[offset + inputChannelCount + inputChannel];
                    else if (this.queue.length > 1)
                        value2 = this.queue[1][inputChannel];
                    else
                        value2 = value1;
                    let value = (value1 + ((value2 - value1) * (nextSamplePosition % 1))) / 32768;
                    output[outputChannel][written] = value;
                }
                written++;
                nextSamplePosition += playbackRate;
            }

            if (nextSamplePosition >= samplesInBuffer)
            {
                nextSamplePosition -= samplesInBuffer;
                this.queue.shift();
                this.port.postMessage(1);
            }

            this.samplePosition = nextSamplePosition;
        }
        
        // Fill remaining samples with silence
        if (written < outputSampleCount)
        {
            for (let outputChannel = 0; outputChannel < outputChannelCount; outputChannel++)
            {
                for (let i = written; i < outputSampleCount; i++)
                {
                    output[outputChannel][i] = 0;
                }
            }
        }

        return true;
    }
}

registerProcessor("stream-processor", StreamProcessor);