
# Project YALLAH - Manual Main Document

This is the first and Main chapter of the Instruction Manual for project YALLAH.
YALLAH stands for **Yet Another Low-Level Agent Handler**.

## Description

The goal of YALLAH is to allow 3D content creators to **generate, customize, dress a virtual human, and deploy it in a Game Engine in few hours of work**.
* For 3D authors, YALLAH is a reference for the development of virtual humans that have to be employed in real-time engines.
* For software developers, YALLAH is an open platform that can be taken as reference to implement new functionalities for improved interactive virtual humans.

We aim at providing a common framework and API for the creation of multi-functional virtual humans that can be used in different application domains: video games, embodied conversational agents, virtual assistants, front-end for chat-bot systems.

The following people can take advantage of project YALLAH:

* **3D Authors**. With a basic knowledge of Blender, authors can create a custom character and have it ready in a game engine like Unity in less than an hour of work.
* **3D Software Developers** can insert a new character in a virtual environment and control it with a high-level API (e.g., LookAt, SpeakText, PlayAnimation, SetFacialExpression, ...).
* **Virtual Humans' Developers and Researchers** can use YALLAH as starting point and develop new motion controllers on top of it.

## Content


The manual is divided in two main sections:
1. The **[Authoring Manual](Manual%20-%20Authoring.md)** explains how to create a new Character in the Blender authoring software.
    1. The **[Unity Authoring Manual](Manual%20-%20Authoring%20-%20Unity%20Deploy.md)** explains how to take a character authored in Blender and have it ready in the Unity game engine.
2. The **[Developer Manual](Manual%20-%20Developer.md)** explains to developers how to script the behavior of YALLAH characters in real-time environments (like Unity or WebPages).
3. The **[Contributor Manual](Manual%20-%20Contributor.md)** is for developers and researchers who want to contribute to the YALLAH project. This manual gives the guidelines to develop new motion controllers.


## Some more details

* For the creation of virtual characters, YALLAH relies on Blender and the ManuelBastioniLAB free add-on.
* A set of additional scripts customize the character to make it ready for use in a real-time engine.
* The generated character can be used in two different real-time engine:
  - The internal Blender Game Engine. So the character can be deployed on desktop platforms with simple file sharing.
  - The Unity Engine. In this configuration the character can be integrated as Asset in any Unity project and deployed on most desktop, mobile, and web platforms.
    - _Features_ like eye-blinking, eye gaze, animated Text-to-speak (and others), can be enabled or disabled according to the needs (application domain, performance, ...)
  - A set of dedicated scripts allow to build a Unity project for WebGL and use the high-level API (LookAt, SpeakText, ...) directly from the web pages via JavaScript code.
* If a developer wants to implement new motion controllers (e.g., locomotion), a set of guidelines and design patterns allow him/her to inject the new feature into the system without breaking the compatibility with existing ones.


## Links

Open Source repository: 

DECAD (DFKI Embodied Conversational Agent Demo) at [http://decad.sb.dfki.de](http://decad.sb.dfki.de) demonstrates an online running demo of a virtual human created with YALLAH.
Please, refer to this web site for latest online demo and downloads.

Software used:
* [Blender](https://www.blender.org/)
* [Manuel Bastioni Laboratory](http://www.manuelbastioni.com/)
* [Unity](https://unity3d.com/)
