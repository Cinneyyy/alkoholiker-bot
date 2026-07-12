rsync -rav -e ssh --exclude="vc_state/*" --exclude="msg_log/*" --exclude="log.txt" --exclude="call_stats/*" --exclude="casino/user_data/*" --exclude="live_stats/*" \
    ~/.config/alkoholiker/ \
    cinneyyy@rb.pi:/home/cinneyyy/.config/alkoholiker
