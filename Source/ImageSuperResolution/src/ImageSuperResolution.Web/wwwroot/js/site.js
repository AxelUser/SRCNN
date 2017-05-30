(function (container) {
    "use strict";
    requirejs.config({
        "paths": {
            "vue": "/lib/vue/dist/vue",
            "vue-resource": "/lib/vue-resource/dist/vue-resource"
        }
    });
    require(['vue', 'vue-resource'],
        (Vue, VueResource) => {

            Vue.use(VueResource);

            var model = new Vue({
                el: "#app",
                data: {
                    image: null,
                    imageHeight: 0,
                    imageWidth: 0,
                    uploadedFile: null,
                    upscaledFile: null,
                    scale: 2,
                    progressRatio: 0,
                    progressMessage: null,
                    upscalledImage: null,
                    isRunning: false,
                    isShownModified: false,
                    progressPooling: null,
                    ticket: null,
                    totalBlocks: null
                },
                computed: {
                    cancelButtonText() {
                        return this.isRunning ? "Cancel" : "Remove image";
                    },
                    toggleButtonText() {
                        return this.isShownModified ? "Show original" : "Show modified";
                    },
                    isReady() {
                        return !this.isRunning && this.upscaledFile !== null;
                    },
                    getImagePreviewSource() {
                        if (this.image !== null) {
                            return this.isShownModified ? this.upscaledFile : this.image.src;
                        } else {
                            return null;
                        }
                    },
                    currentImageHeight() {
                        return this.isShownModified ? this.imageHeight * 2 : this.imageHeight;
                    },
                    currentImageWidth() {
                        return this.isShownModified ? this.imageWidth * 2 : this.imageWidth;
                    }
                },
                methods: {
                    toggleImage() {
                        this.isShownModified = !this.isShownModified;
                    },
                    uploadFile() {
                        this.$http.post("/api/Upscalling/Upload", this.uploadedFile)
                            .then(response => {  
                                this.ticket = response.body;
                            }, response => console.log("Upload Fail"));
                    },
                    getProgress() {
                        this.$http.get("/api/Upscalling/GetProgress", { params: { ticket: this.ticket } })
                            .then(response => {
                                if (response.body) {
                                    console.log(response.body);
                                    let statusObject = this.getTaskStatus(response.body);
                                    this.handleProgress(statusObject);
                                }                                
                            }, response => console.log("Progress Fail"));
                    },
                    getTaskStatus(messages = []) {
                        const status = {
                            isReceived: false,
                            isDecomposing: false,
                            isUpscalling: false,
                            isComposing: false,
                            isReady: false
                        }
                        let totalBLocks = null;
                        const blocksProcessed = [];
                        let upscaledImageUrl = null;

                        messages.forEach(message => {
                            switch (message.status) {
                                //Image received
                                case 0:
                                    status.isReceived = true;
                                    break;
                                //Decomposing
                                case 1:
                                    status.isDecomposing = true;
                                    break;
                                //Upscalling blocks
                                case 2:
                                    status.isUpscalling = true;
                                    totalBLocks = message.blocksCount;
                                    blocksProcessed.push(message.blockNumber);
                                    break;
                                //Composing
                                case 3:
                                    status.isComposing = true;
                                    break;
                                //Upscaled image was sent
                                case 4:
                                    status.isReady = true;
                                    break;
                            }
                        });
                        return {
                            status,
                            totalBLocks,
                            blocksProcessed,
                            upscaledImageUrl
                        }
                    },
                    handleProgress(taskStatusObject) {
                        let text = "Waiting";
                        let url = null;
                        if (taskStatusObject.status.isReceived) {
                            text = "Image was received";
                            if (taskStatusObject.status.isDecomposing) {
                                text = "Decomposing";
                                if (taskStatusObject.status.isUpscalling) {
                                    text = `Upscalling: ${taskStatusObject.blocksProcessed.length} of ${taskStatusObject.totalBLocks}`;
                                    if (taskStatusObject.status.isComposing) {
                                        text = "Composing";
                                        if (taskStatusObject.status.isReady) {
                                            text = "Ready";
                                            this.upscalledImage = taskStatusObject.upscaledImageUrl;
                                        }
                                    }
                                }
                            }
                        }
                        this.progressMessage = text;
                    },
                    getResult() {
                        this.$http.get("/api/Upscalling/GetResult", { params: { ticket: this.ticket } })
                            .then(response => {
                                if (response.body) {
                                    console.log(response.body);
                                }
                            }, response => console.log("Rusult Fail"));
                    },
                    initPooling() {
                        this.progressPooling = setInterval(() => this.getProgress(), 5000);
                    },
                    stopPooling() {
                        clearInterval(this.progressPooling());
                    },
                    startProcess() {
                        let canvas = document.createElement('canvas');
                        let context = canvas.getContext('2d');
                        canvas.width = this.imageWidth;
                        canvas.height = this.imageHeight;
                        context.drawImage(this.image, 0, 0);
                        let imageData = context.getImageData(0, 0, this.imageWidth, this.imageHeight);
                        this.uploadFile();
                        this.isRunning = true;
                        this.initPooling();
                    },
                    onFileChange(e) {
                        let files = e.target.files || e.dataTransfer.files;
                        if (!files.length)
                            return;
                        this.createImage(files[0]);
                        const uploadedFile = new FormData();
                        uploadedFile.append("Image", files[0]);
                        this.uploadedFile = uploadedFile;
                    },
                    createImage(file) {
                        const reader = new FileReader();
                        const vm = this;
                        reader.onload = (file) => {
                            this.image = new Image();
                            this.image.onload = function() {
                                vm.imageWidth = this.naturalWidth;
                                vm.imageHeight = this.naturalHeight;
                            };
                            this.image.src = file.target.result;
                        };
                        reader.readAsDataURL(file);
                    },
                    cancel() {
                        if (this.isRunning) {
                            this.stopWorker();
                        } else {
                            this.image = null;
                            this.upscaledFile = null;
                            this.progressMessage = null;
                        }
                        this.isShownModified = false;
                    }
                },
                mounted() {
                }
            });
        });
})(window);
