# Copyright (c) 2025, Dassault Systèmes SE
# All rights reserved.
#
# Redistribution and use permitted under the terms of the 3-clause BSD license.

TTY:=-it
ifdef NO_TTY
	TTY:=-t
endif

#:help: help        | Displays the GNU makefile help
.PHONY: help
help: ; @sed -n 's/^#:help://p' Makefile

#:help: up          | Starts up a NuoDB cluster
.PHONY: up
up:
	scripts/up

#:help: status      | Shows the NuoDB cluster status
.PHONY: status
status:
	scripts/status

#:help: term        | Opens up a terminal to a running NuoDB cluster
.PHONY: term
term:
	@echo "Welcome to NuoDB"
	@echo ""
	@echo "   To open a SQL prompt run the following command:"
	@echo "      nuosql test@ad1 --user dba --password dba"
	@echo ""
	@echo "   To show what's running in the domain run the following command:"
	@echo "      nuocmd --api-server ad1:8888 show domain"
	@echo ""
	@echo "   To get out the terminal simply type exit."
	@echo ""
	@scripts/term

#:help: dn          | Stops the NuoDB cluster
.PHONY: dn
dn:
	scripts/dn
