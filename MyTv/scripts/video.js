
var vid = null;

function init() {

    vid = document.getElementById("video1");
    vid.addEventListener("timeupdate", seektimeupdate, false);

    document.addEventListener('mousedown', down, false);
    document.getElementById('video_controls_bar').addEventListener('mouseover', over, false);
    document.getElementById('video_controls_bar').addEventListener('mouseout', out, false);
    
    playPause();
}

function down() {

    if ($('#video_controls_bar').css('opacity') == 0) {
        $('#video_controls_bar').css('opacity', 1);
    }
}

function over() {
    $('#video_controls_bar').css('opacity', 1);
}

function out() {
    setTimeout(function () { $('#video_controls_bar').css('opacity', 0); }, 5000);
}

function playPause() {
    if (vid == null) init();

    if (vid.paused) {
        vid.play();
        $('#play').attr('src', 'pause.gif');
    }
    else {
        vid.pause();
        $('#play').attr('src', 'play.gif');
    }
    setTimeout(function () { $('#video_controls_bar').css('opacity', 0); }, 5000);
}

function toggleScreen() {
 	if (vid == null) init();

    if (vid.requestFullScreen) {
        vid.requestFullScreen();
    } else if (vid.webkitRequestFullScreen) {
        vid.webkitRequestFullScreen();
    } else if (vid.mozRequestFullScreen) {
        vid.mozRequestFullScreen();
    }
}

function vidSeek() {
 	if (vid == null) init();

    var seekto = vid.duration * (seekslider.value / 100);
    vid.currentTime = seekto;
}

function seektimeupdate() {

    var nt = vid.currentTime * (100 / vid.duration);
    seekslider.value = nt;
    var curmins = Math.floor(vid.currentTime / 60);
    var cursecs = Math.floor(vid.currentTime - curmins * 60);
    var durmins = Math.floor(vid.duration / 60);
    var dursecs = Math.floor(vid.duration - durmins * 60);
    if (cursecs < 10) {
        cursecs = "0" + cursecs;
    }
    if (dursecs < 10) {
        dursecs = "0" + dursecs;
    }
    if (curmins < 10) {
        curmins = "0" + curmins;
    }
    if (durmins < 10) {
        durmins = "0" + durmins;
    }
    curtimetext.innerHTML = curmins + ":" + cursecs;
    durtimetext.innerHTML = durmins + ":" + dursecs;
}
