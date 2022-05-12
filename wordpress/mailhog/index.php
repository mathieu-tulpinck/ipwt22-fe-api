<?php
/*
Plugin Name: Mailhog
*/
add_action('phpmailer_init', 'setup');

function setup(\PHPMailer\PHPMailer\PHPMailer $phpmailer) {
    $phpmailer->Host = 'mailhog';
    $phpmailer->Port = 1025;
    $phpmailer->IsSMTP();
}