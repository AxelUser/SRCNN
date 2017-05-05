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
                    isRunning: false,
                    isShownModified: false
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
                            .then(response => alert("Uploaded"), response => alert("Fail"));
                    },
                    startProcess() {
                        let canvas = document.createElement('canvas');
                        let context = canvas.getContext('2d');
                        canvas.width = this.imageWidth;
                        canvas.height = this.imageHeight;
                        context.drawImage(this.image, 0, 0);
                        let imageData = context.getImageData(0, 0, this.imageWidth, this.imageHeight);
                        this.isRunning = true;
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
