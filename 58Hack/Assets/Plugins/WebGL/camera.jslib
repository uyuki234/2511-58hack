mergeInto(LibraryManager.library, {
    GetBackCameraStream: function () {
        navigator.mediaDevices.getUserMedia({
            video: { facingMode: { exact: "environment" } }
        }).then(stream => {
            let video = document.getElementById("unity-video");
            if (!video) {
                video = document.createElement("video");
                video.id = "unity-video";
                video.autoplay = true;
                video.playsInline = true;
                document.body.appendChild(video);
            }
            video.srcObject = stream;
        }).catch(err => {
            console.error("Back camera not available:", err);
        });
    }
});
