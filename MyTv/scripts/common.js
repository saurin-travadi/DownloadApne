
var source = "GetBollyStop";
//var root = "http://localhost:33619/";
var root = "http://mytvweb.azurewebsites.net/";

function menuClick() {
    if ($('#menuspan').is(':visible')) {
        $('.header').animate({ height: '60px' }, 2000);
        $('#menuspan').hide(2000);
    }
    else {
        $('.header').animate({ height: '100%' }, 2000);
        $('#menuspan').show(2000);
    }
    return false;
}

function goHome() {
    window.location = "Default.html";
}

function getChannel() {

    setContentDiv();

    if (getUrlVars()["source"] != null)
        source = unescape(getUrlVars()["source"]);

    var script = root + "GetShow.aspx?source=" + source;
    $.getScript(script, function (data, textStatus, jqxhr) {
        var obj = myTVScript.serials.filter(function (e) { return e.Source.toLowerCase() == source.toLowerCase() });
        var ch = obj[0].Channel;
        $.each(ch, function (i, c) {

            if (i != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + i);
                cd.html(cd.html().replace('a-0', 'a-' + i).replace('span-0', 'span-' + i).replace('img-0', 'img-' + i).replace('ui-btn-icon-right', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + i).attr('href', 'show.html?source=' + source + '&s=' + escape(c.Name));
            $('#img-' + i).attr('src', c.ImageURL);
            $('#span-' + i).html(c.Name);
        });

        HideProgress('content');
    });
}

function getChannel1() {

    setContentDiv();

    if (getUrlVars()["source"] != null)
        source = unescape(getUrlVars()["source"]);

    var script = root + "GetShow.aspx?source=" + source;
    $.getScript(script, function (data, textStatus, jqxhr) {
        var obj = myTVScript.serials.filter(function (e) { return e.Source.toLowerCase() == source.toLowerCase() });
        var ch = obj[0].Channel;
        $.each(ch, function (i, c) {

            var h3 = $('<h3>');
            $(h3).html(c.Name);
            $('#accordion').append(h3);

            var div = $('<div>');
            $(div).append($('<button style="float:left">&lt;</button>'));
            var d1 = $('<div class="scroll-pane ui-widget ui-widget-header ui-corner-all">');
            var d2 = $('<div class="scroll-content">');
            $.each(c.Shows, function (j, s) {

                var a = '';
                if (source == 'GetYoDesi') {
                    a = $('<a href="yodesi.html?url=' + escape(s.URL) + '">' + s.Name + '</a>');
                }
                else {
                    a = $('<a href="date.html?source=' + source + '&s=' + escape(s.Name) + '&url=' + escape(s.URL) + '">' + s.Name + '</a>');
                }
                var show = $('<div class="scroll-content-item ui-widget-header">');
                $(show).append(a);
                $(d2).append(show);
            });
            $(d2).css('width', c.Shows.length * 180);
            $(d1).append(d2);
            $(div).append(d1);
            $(div).append($('<button style="float:left">&gt;</button>'));
            $('#accordion').append(div);

        });

        $("#accordion").accordion({
            event: "click hoverintent",
            heightStyle: "content"
        });

        HideProgress('content');
    });
}

function getShow() {

    setContentDiv();

    source = unescape(getUrlVars()["source"]);
    var show = unescape(getUrlVars()["s"]);

    var script = root + "GetShow.aspx?source=" + source;
    $.getScript(script, function (data, textStatus, jqxhr) {

        var obj = myTVScript.serials.filter(function (e) { return e.Source.toLowerCase() == source.toLowerCase() });
        var sh = obj[0].Channel.filter(function (e) { return e.Name == show });

        $.each(sh[0].Shows, function (i, s) {

            if (i != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + i);
                cd.html(cd.html().replace('a-0', 'a-' + i).replace('span-0', 'span-' + i).replace('ui-btn-icon-right', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + i).attr('href', 'url.html?source=' + source + '&s=' + escape(s.Name) + '&url=' + escape(s.URL));
            $('#span-' + i).html(s.Name);
        });
        HideProgress('content');
    });
}

function getDate() {

    setContentDiv();

    var show = getUrlVars()["s"];
    var url = getUrlVars()["url"];
    source = getUrlVars()["source"];

    var script = root + "GetDate.aspx?s=" + show + "&url=" + url + "&source=" + source;
    $.getScript(script, function (data, textStatus, jqxhr) {

        $.each(myTVScript.dates, function (index, elem) {

            if (index != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + index);
                cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + index).attr('href', 'video.html?source=' + source + '&s=' + show + '&d=' + elem + '&url=' + url);
            $('#span-' + index).html(elem);
        });
        HideProgress('content');
    });
}

function getFormat() {

    setContentDiv();

    var show = getUrlVars()["s"];
    var date = getUrlVars()["d"];
    var url = getUrlVars()["url"];
    source = getUrlVars()["source"];

    var formates = ['TuneLink', 'PlayApne', 'SaveBox', 'Telly', 'Speedwatch', 'Flash', 'WatchApne', 'Video'];
    $.each(formates, function (index, elem) {

        if (index != 0) {
            var od = $('#div-0');
            var cd = $(od).clone();

            $(cd).attr('id', 'div-' + index);
            cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', ''));

            $(od).parent().append(cd);
        }

        $('#a-' + index).attr('href', 'url.html?source=' + source + '&s=' + escape(show) + '&d=' + date + '&url=' + url + '&f=' + elem);
        $('#span-' + index).html(elem);
    });
    HideProgress('content');
}

function getVideo() {

    ShowProgress('showvideoplayer');

    var show = unescape(getUrlVars()["s"]);
    var date = unescape(getUrlVars()["d"]);
    var refurl = getUrlVars()["url"];
    var source = getUrlVars()["source"];
    var format = unescape(getUrlVars()["f"]);

    var url = root + "GetURL.aspx?s=" + show + "&d=" + date + "&source=" + source + "&url=" + refurl + "&f=" + format;
    $.getScript(url, function (data, textStatus, jqxhr) {

        HideProgress('showvideoplayer');

        var sp = $('<span>').appendTo($('.header1'));
        if (myTVScript == null || myTVScript.url == null || myTVScript.url == '')
            alert('No video found');
        else {

            var h = screen.height;
            var w = screen.width;

            var txt = ["Link 1", "Link 2", "Link 3", "Link 4", "Link 5", "Link 6", "Link 7", "Link 8"];
            var cnt = 0;
            $.each(myTVScript.url.split('|'), function (index, elem) {
                var cls = 'vid';

                if (myTVScript.url.split('|').length > 1) {

                    var lnk = $("<a></a>").attr("class", "lnk " + cls + (index == 0 ? " sel" : "")).attr('href', '#lnk' + index).attr("rel", elem).text(txt[cnt++]);
                    $(sp).append(lnk);
                }

                if (index == 0) setJWPlayerWithURL(elem, myTVScript.isDM == 'true' ? 1 : 0);
            });
        }
        var vAll = "date.html?s=" + show + "&url=" + refurl + "&source=" + source;
        lnk = $("<a></a>").attr("class", "lnk").attr('href', vAll).text('View All');
        $(sp).append(lnk);
    })
    .fail(function () {
        HideProgress('showvideoplayer');
    });
}

function getYoDesiVideo() {

    ShowProgress('showvideoplayer');

    var url = getUrlVars()["url"];

    $.getJSON('http://whateverorigin.org/get?url=' + encodeURIComponent(unescape(url)) + '&callback=?', function (data) {
        data = data.contents;
        data = data.replace(/(\r\n|\n|\r)/gm, "");

        var reg = reg = /<div class="buttons btn_green"><span class="single-heading">Dailymotion 720p HD Videos.*<\/a><\/p><div class="buttons btn_green"><span class="single-heading">WatchVideo 720p HD Videos/;
        var matches = data.match(reg);

        var str = matches[0];
        str = str.replace('<div class="buttons btn_green"><span class="single-heading">Dailymotion 720p HD Videos</span></div>', '');
        str = str.replace('<div class="buttons btn_green"><span class="single-heading">WatchVideo 720p HD Videos', '');

        alert(str);
        //window.location = str;
    });
}

function show(p) {

    var format = unescape(getUrlVars()["f"]);
    if (format == 'Telly' || format == 'Telly#') {
        setupJWPlayer(p == 1 ? myTVScript.url : myTVScript.url1);
    }
    else {
        setupPlayer(p == 1 ? myTVScript.url : myTVScript.url1);
    }
}

function getNewsChannle() {

    setContentDiv();

    var formates = ['NDTV 24x7', 'NDTV India', 'Zee News', 'Times NOW', 'India TV', 'Aaj Tak'];
    $.each(formates, function (index, elem) {

        if (index != 0) {
            var od = $('#div-0');
            var cd = $(od).clone();

            $(cd).attr('id', 'div-' + index);
            cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', ''));

            $(od).parent().append(cd);
        }

        $('#a-' + index).attr('href', 'newsvideo.html?source=' + elem.replace(' ', '_'));
        $('#span-' + index).html(elem);
    });
    HideProgress('content');
}

function playNewsLive(p) {

    $.getScript(root + "GetNews.aspx?source=Live", function (data, textStatus, jqxhr) {
        var curVideo = myTVScript.movies.filter(function (element) { return element.MovieName == p });
        if (p == 'Aaj_Tak_TEZ') {
            document.getElementById('jwplayer').style.display = "none";
            document.getElementById('id6').style.display = "block";
            document.getElementById('id6').src = curVideo[0].MovieURL;
        }
        else {
            document.getElementById('id6').style.display = "none";
            document.getElementById('jwplayer').style.display = "block";
            setJWPlayerWithURL(curVideo[0].MovieURL, 0);
        }
    });
}


var isVideoLoaded = false;
function setupPlayer(url) {

    var h = screen.height - 200;
    var w = screen.width - 100;

    if (isVideoLoaded) {

        var videocontainer = document.getElementById("video1");
        videocontainer.pause();
        videosource.setAttribute('src', url);
        videocontainer.load();
        videocontainer.play();

    }
    else {

        $('#video-container').show();

        $('#video1').append('<source id="videosource" type="video/mp4" src="' + url + '"></source>');
        $('#video1').css('height', h);
        $('#video1').css('width', w);
        $('#video_controls_bar').css('left', (w - 1400) / 2);

        init();
        isVideoLoaded = true;
    }
}

function setupJWPlayer(url) {

    if ($('#showvideoplayer_wrapper').length != 0) {
        $('#showvideoplayer_wrapper').remove();
        $("<div id='showvideoplayer' />").appendTo('body');
    }
    setJWPlayerWithURL(url, 0);
}

function getMovie(p, isMovie) {

    var script = root;
    if (isMovie == 0) script += "GetVideo.aspx?";
    if (isMovie == 1) script += "GetMovies.aspx?"
    if (isMovie == 2) script += "GetNews.aspx?";
    if (isMovie == 3) script += "GetCricket.aspx?";
    if (getUrlVars()["source"] != null) script += "source=" + getUrlVars()["source"] + "&";
    if ($('#hdnShow').length > 0) script += "source=" + $('#hdnShow').val() + "&";

    var page = 1;
    if (p == 0) {
        var search = $('input[type="text"]').val();
        script += "search=" + search;
    }
    else {
        if ($('#hdnPage').length > 0)
            page = $('#hdnPage').val();

        page = eval(page) + p;
        script += "page=" + page;
    }

    if ($('#hdnPage').length > 0) $('#hdnPage').val(page);
    setContentDiv();

    ShowProgress('list');
    $.getScript(script, function (data, textStatus, jqxhr) {

        $('#list').find('div:not("#div-0")').remove();

        $.each(myTVScript.movies, function (index, elem) {

            if (index != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + index);
                cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('img-0', 'img-' + index).replace('ui-btn-icon-right', ''));

                $(od).parent().append(cd);
            }

            var href = '?m=' + elem.MovieURL;
            if (getUrlVars()["source"] != null) href += "&source=" + getUrlVars()["source"];
            //$('#a-' + index).attr('href', href).attr("onclick", "playMovieInModal('" + elem.MovieURL + "','" + getUrlVars()["source"] + "');return false;");
            $('#a-' + index).attr('href', href);

            var name = elem.MovieName;
            var reg = new RegExp("Full Movie.+$");
            var search = name.match(reg);
            name = name.replace(search, '')
            $('#span-' + index).html(name);
            $('#img-' + index).attr('src', elem.MovieImage);
        });

        HideProgress('list');
    })
    .fail(function () {
        HideProgress('list');
    });
}

function playMovie(p) {

    var source = getUrlVars()["source"];
    if (source == undefined) source = $('#hdnShow').val();

    var m = getUrlVars()["m"];
    var url = root;
    if (p == undefined) url += "GetMovies.aspx?m=";
    if (p == 1) url += "GetNews.aspx?source=" + source + "&m=";
    if (p == 3) url += "GetCricket.aspx?source=" + source + "&m=";
    url += m;

    ShowProgress('list');
    $.getScript(url, function (data, textStatus, jqxhr) {

        if (myTVScript == null || myTVScript.movies == null || myTVScript.movies == '')
            alert('No video found');
        else {

            var h = screen.height;
            var w = screen.width;
            var cookies = myTVScript.cookieString.split('^');
            for (p in cookies) {
                document.cookie = p;
            }

            var txt = ["Link 1", "Link 2", "Link 3", "Link 4", "Link 5", "Link 6", "Link 7", "Link 8"];
            var cnt = 0;
            var sp = $('<span>').appendTo($('.header1'));
            $.each(myTVScript.movies, function (index, elem) {
                var cls = 'vid';

                if (myTVScript.movies.length > 1) {
                    if (elem.IsVideoURL == 0) cls = 'pge';

                    if (!elem.Embed || elem.MovieURL.toLowerCase().indexOf('html') > 0) {
                        var lnk = $("<a></a>").attr("class", "pop " + cls + (index == 0 ? " sel" : "")).attr('href', elem.MovieURL).attr('target', '_blank').text(txt[cnt++]);
                        $(sp).append(lnk);
                    }
                    else {
                        var lnk = $("<a></a>").attr("class", "lnk " + cls + (index == 0 ? " sel" : "")).attr('href', '#lnk' + index).attr("rel", elem.MovieURL).text(txt[cnt++]);
                        $(sp).append(lnk);
                    }
                }

                if (index == 0 && elem.Embed) setJWPlayerWithURL(myTVScript.movies[0].MovieURL, !elem.IsVideoURL);
            });
        }

        HideProgress('list');
    });
}

function playMovieInModal(m, source) {

    var h = screen.height;
    var w = screen.width;
    if ($('.header1').is(':visible')) h = h - $('.header1').height();
    if ($('.header').is(':visible')) h = h - $('.header').height();

    $('<div id=openwindows>').appendTo($('#showvideoplayer'));
    $('#openwindows').dialog({
        modal: true,
        height: h * 0.75,
        width: w * 0.75
    });
    $('#openwindows').dialog('open');
}

function setJWPlayerWithURL(url, isPage) {

    var h = screen.height;
    var w = screen.width;
    if ($('.header1').is(':visible')) h = h - $('.header1').height();
    if ($('.header').is(':visible')) h = h - $('.header').height();

    if (isPage == 1) {
        $('#showvideoplayer iframe').remove();
        $('<iframe src="' + url + '" Height="' + h * 0.75 + '" Width="' + w * 0.75 + '" frameborder="0" scrolling="no" allowfullscreen style="display:block;margin-left:auto;margin-right:auto;"></iframe>').appendTo('#showvideoplayer');
    }
    else {
        var p = $('#showvideoplayer_wrapper').parent();
        $(p).append('<div id="showvideoplayer"></div>');
        $('#showvideoplayer_wrapper').remove();

        flowplayer.conf.share = false;
        flowplayer.conf.fullscreen = true;
        flowplayer.conf.native_fullscreen = true;
        var container = document.getElementById("showvideoplayer");
        var p = flowplayer(container, {
            clip: {
                sources: [
                    {
                        src: url,
                        type: "application/x-mpegurl"
                    }
                ]
            },
            screen: {
                width: 387, height: 231, top: 55, right: 77
            },
            share: false
        }).on("ready", function (e, api) {
            var fsbutton = container.querySelector(".fp-fullscreen");
            container.querySelector(".fp-controls").appendChild(fsbutton);
        });
        $('.flowplayer').height(h * 0.75);
        $('.flowplayer').width(w * 0.75);

        //$('.jw-button-container').css('width', '100');
        //$('.jw-controlbar').css('width', '100');
        //jwplayer("showvideoplayer").setup({
        //    file: url,
        //    hlshtml: true,
        //    autoStart: true,
        //    primary: 'html5',
        //    cookies: true,
        //    width: w * 0.75,
        //    height: h * 0.75,
        //    title: "Click Play"
        //});
    }
}

function getRadio() {

    var formates = ['http://tunein.com/embed/player/s129208/', 'http://tunein.com/embed/player/s68483/'];
    $.each(formates, function (index, elem) {

        if (index != 0) {
            var od = $('#div-0');
            var cd = $(od).clone();

            $(cd).attr('id', 'div-' + index);
            cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', ''));

            $(od).parent().append(cd);
        }

        $('#a-' + index).attr('href', '#');
        $('#span-' + index).html('<iframe src="' + elem + '" />');
    });

}

function setContentDiv() {
    var h = screen.height;
    var w = screen.width;
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function move(go) {
    if (go == 'd')
        $('#lstShow').scrollTop($('#lstShow').scrollTop() + 20);
    else
        $('#lstShow').scrollTop($('#lstShow').scrollTop() - 20);

}

function ShowProgress(elementId) {
    $("#" + elementId).css("position", "relative");

    if (!($("#" + elementId).find("div.progress-bar")[0])) {
        $("#" + elementId).prepend("<div class='progress-bar'><div class='progress-bar-img'>&nbsp;</div></div>");
    }
}

function HideProgress(elementId) {
    $("#" + elementId).find(".progress-bar").remove();
}

function getSite() {
    var url = getUrlVars()["m"];

    //$.getJSON('http://whateverorigin.org/get?url=' + encodeURIComponent(url) + '&callback=?', function (data) {
    //    data = data.contents;
    //    var reg = new RegExp("https://openload.co/embed/.+scrolling");
    //    var matches = data.match(reg);

    //    var str = matches[0];
    //    str = str.substring(0, str.length).replace('/"', '').replace('scrolling', '');
    //    window.location = str;
    //});

    $.getJSON('http://whateverorigin.org/get?url=' + encodeURIComponent(url) + '&callback=?', function (data) {
        debugger;

        var arr = new Array();

        data = data.contents;
        var reg = new RegExp("https://openload.co/embed/.+scrolling");
        var matches = data.match(reg);

        var str = matches[0];
        str = str.substring(0, str.length).replace('/"', '').replace('scrolling', '');
        arr.push(str);

        //reg = new RegExp("http://watchvideo5.us/.?html");
        //var matches = data.match(reg);

        //var str = matches[0];
        //str = str.substring(0, str.length).replace('/"', '').replace('html', '');
        //arr.push(str);


        window.location = str;
    });

    return false;
}

function showFav() {
    var ck = getCookie('fav_shows');
    if (ck != '') {
        var obj = JSON.parse(ck);

        if (obj.length == 0) {
            $('#div-0').parent().append($('span').html('You have no favorites, add for quicker browsing...'))
            $('#div-0').hide();
        }
        for (i = 0; i < obj.length; i++) {
            if (i != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + i);
                cd.html(cd.html().replace('a-0', 'a-' + i).replace('span-0', 'span-' + i).replace('ui-btn-icon-right', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + i).attr('href', 'url.html?' + obj[i].url);
            $('#span-' + i).html(unescape(obj[i].name));
        }
    }
}

function add2Fav() {
    var ck = getCookie('fav_shows');
    var obj = [];
    if (ck != '') {
        obj = JSON.parse(ck);
    }

    obj.push({ name: getUrlVars()['s'], url: window.location.href.slice(window.location.href.indexOf('?') + 1) });
    var strObj = JSON.stringify(obj);
    setCookie('fav_shows', strObj, 365);
}

function resetFav(e) {
    $(e).parent().parent().remove();
    var d = $("div[id^='div-']");

    var obj = [];
    $("div[id^='div-']").each(function (i, e) {
        var url = $(e).find('a').attr('href');
        var txt = $(e).find('a span').text();
        obj.push({ name: txt, url: url });
    });
    var strObj = JSON.stringify(obj);
    setCookie('fav_shows', strObj, 365);
}

function goNext() { alert('Need to go Next day'); }

function goPrev() { alert('Need to go Previous day'); }

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
