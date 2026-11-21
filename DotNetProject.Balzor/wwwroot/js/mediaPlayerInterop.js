/**
 * NangWorld Media Player JSInterop Module
 * Tracks playback timestamps with 5s throttling, auto-seeks on metadata loaded,
 * and handles subtitle track toggling.
 */

window.mediaPlayer = (function () {
    const players = new Map();

    return {
        /**
         * Initialize player progress tracker and auto-seek
         */
        initPlayer: function (videoElementId, dotNetRef, resumeTimestampSeconds) {
            const video = document.getElementById(videoElementId);
            if (!video) {
                console.warn(`[mediaPlayer] Video element '${videoElementId}' not found.`);
                return;
            }

            // Cleanup previous listeners if initialized
            this.disposePlayer(videoElementId);

            let lastSavedTime = 0;
            const THROTTLE_INTERVAL_MS = 5000;
            let lastCallTimestamp = 0;

            const sendProgress = (force = false) => {
                if (!video || isNaN(video.currentTime)) return;
                const now = Date.now();
                const currentSec = Math.floor(video.currentTime);
                const durationSec = Math.floor(video.duration || 0);

                if (force || (now - lastCallTimestamp >= THROTTLE_INTERVAL_MS && Math.abs(currentSec - lastSavedTime) >= 3)) {
                    lastCallTimestamp = now;
                    lastSavedTime = currentSec;
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnPlaybackProgress', currentSec, durationSec)
                            .catch(err => console.debug('[mediaPlayer] OnPlaybackProgress callback error:', err));
                    }
                }
            };

            const onTimeUpdate = () => sendProgress(false);
            const onPauseOrEnd = () => sendProgress(true);

            // Auto-seek to resume timestamp once metadata is available
            const applyAutoSeek = () => {
                if (resumeTimestampSeconds && resumeTimestampSeconds > 5) {
                    const duration = video.duration || 0;
                    // If progress < 90% of duration, auto-seek
                    if (duration === 0 || resumeTimestampSeconds < (duration * 0.90)) {
                        try {
                            video.currentTime = resumeTimestampSeconds;
                            console.log(`[mediaPlayer] Auto-resumed playback from ${resumeTimestampSeconds}s.`);
                        } catch (e) {
                            console.debug('[mediaPlayer] Could not set currentTime:', e);
                        }
                    }
                }
            };

            if (video.readyState >= 1) {
                applyAutoSeek();
            } else {
                video.addEventListener('loadedmetadata', applyAutoSeek, { once: true });
            }

            video.addEventListener('timeupdate', onTimeUpdate);
            video.addEventListener('pause', onPauseOrEnd);
            video.addEventListener('ended', onPauseOrEnd);

            players.set(videoElementId, {
                video: video,
                onTimeUpdate: onTimeUpdate,
                onPauseOrEnd: onPauseOrEnd,
                applyAutoSeek: applyAutoSeek
            });
        },

        /**
         * Seeks the video to the specified seconds
         */
        seekTo: function (videoElementId, seconds) {
            const video = document.getElementById(videoElementId);
            if (video && !isNaN(seconds)) {
                video.currentTime = Math.max(0, seconds);
            }
        },

        /**
         * Returns current playback time in seconds
         */
        getCurrentTime: function (videoElementId) {
            const video = document.getElementById(videoElementId);
            return video ? Math.floor(video.currentTime || 0) : 0;
        },

        /**
         * Sets subtitle track mode ('showing' or 'disabled')
         */
        setSubtitleTrack: function (videoElementId, lang) {
            const video = document.getElementById(videoElementId);
            if (!video || !video.textTracks) return;

            for (let i = 0; i < video.textTracks.length; i++) {
                const track = video.textTracks[i];
                if (lang === 'off') {
                    track.mode = 'disabled';
                } else if (track.language === lang || track.label.toLowerCase().includes(lang.toLowerCase())) {
                    track.mode = 'showing';
                } else {
                    track.mode = 'disabled';
                }
            }
        },

        /**
         * Removes listeners and cleans up
         */
        disposePlayer: function (videoElementId) {
            const state = players.get(videoElementId);
            if (state && state.video) {
                state.video.removeEventListener('timeupdate', state.onTimeUpdate);
                state.video.removeEventListener('pause', state.onPauseOrEnd);
                state.video.removeEventListener('ended', state.onPauseOrEnd);
                players.delete(videoElementId);
            }
        }
    };
})();
